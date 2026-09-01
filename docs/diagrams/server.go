package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"time"
)

// noCacheHandler makes the browser revalidate every file (HTML, JS, CSS, JSON) it reads.
func noCacheHandler(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Cache-Control", "no-cache, no-store, must-revalidate")
		w.Header().Set("Pragma", "no-cache")
		w.Header().Set("Expires", "0")
		next.ServeHTTP(w, r)
	})
}

func main() {
	var port string
	var rootDir string
	flag.StringVar(&port, "port", "8777", "Port to run the visualizer server on")
	flag.StringVar(&rootDir, "root", "../../", "Root directory to resolve source code files from")
	flag.Parse()

	absRoot, err := filepath.Abs(rootDir)
	if err != nil {
		log.Fatalf("Failed to resolve root directory: %v", err)
	}

	url := fmt.Sprintf("http://localhost:%s/app/", port)

	fmt.Printf("🚀 Starting SPLA Visualizer server on %s\n", url)
	fmt.Printf("📂 Source code root: %s\n", absRoot)
	fmt.Println("Press Ctrl+C to stop.")

	go func() {
		time.Sleep(500 * time.Millisecond)
		openBrowser(url)
	}()

	http.Handle("/", noCacheHandler(http.FileServer(http.Dir("."))))

	http.HandleFunc("/api/source", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet && r.Method != http.MethodHead {
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
			return
		}

		filePath := strings.TrimSpace(r.URL.Query().Get("file"))
		if filePath == "" {
			http.Error(w, "Missing file parameter", http.StatusBadRequest)
			return
		}

		cleanPath := filepath.Clean(filepath.FromSlash(filePath))
		if filepath.IsAbs(cleanPath) {
			http.Error(w, "Absolute paths not allowed", http.StatusBadRequest)
			return
		}

		targetPath := filepath.Join(absRoot, cleanPath)
		absTarget, err := filepath.Abs(targetPath)
		if err != nil {
			http.Error(w, "Failed to resolve path", http.StatusBadRequest)
			return
		}

		relToRoot, err := filepath.Rel(absRoot, absTarget)
		if err != nil || strings.HasPrefix(relToRoot, ".."+string(filepath.Separator)) || relToRoot == ".." {
			http.Error(w, "Access denied: file outside root", http.StatusForbidden)
			return
		}

		info, err := os.Stat(absTarget)
		if err != nil || info.IsDir() {
			http.Error(w, fmt.Sprintf("Source file not found: %s", cleanPath), http.StatusNotFound)
			return
		}

		if r.Method == http.MethodHead {
			w.Header().Set("Content-Length", fmt.Sprintf("%d", info.Size()))
			w.Header().Set("Content-Type", "text/plain; charset=utf-8")
			w.WriteHeader(http.StatusOK)
			return
		}

		data, err := os.ReadFile(absTarget)
		if err != nil {
			http.Error(w, "Failed to read file: "+err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "text/plain; charset=utf-8")
		w.Header().Set("Cache-Control", "no-cache")
		w.WriteHeader(http.StatusOK)
		w.Write(data)
	})

	http.HandleFunc("/api/source/check", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost && r.Method != http.MethodGet {
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
			return
		}

		var filesToCheck []string
		if r.Method == http.MethodGet {
			fileParam := strings.TrimSpace(r.URL.Query().Get("file"))
			if fileParam != "" {
				filesToCheck = append(filesToCheck, fileParam)
			}
		} else {
			body, err := io.ReadAll(r.Body)
			if err == nil && len(body) > 0 {
				_ = json.Unmarshal(body, &filesToCheck)
			}
		}

		result := make(map[string]bool, len(filesToCheck))
		for _, f := range filesToCheck {
			cleanPath := filepath.Clean(filepath.FromSlash(strings.TrimSpace(f)))
			if cleanPath == "" || cleanPath == "." || filepath.IsAbs(cleanPath) {
				result[f] = false
				continue
			}

			targetPath := filepath.Join(absRoot, cleanPath)
			absTarget, err := filepath.Abs(targetPath)
			if err != nil {
				result[f] = false
				continue
			}

			relToRoot, err := filepath.Rel(absRoot, absTarget)
			if err != nil || strings.HasPrefix(relToRoot, ".."+string(filepath.Separator)) || relToRoot == ".." {
				result[f] = false
				continue
			}

			info, err := os.Stat(absTarget)
			result[f] = (err == nil && !info.IsDir())
		}

		w.Header().Set("Content-Type", "application/json; charset=utf-8")
		w.Header().Set("Cache-Control", "no-cache")
		w.WriteHeader(http.StatusOK)
		_ = json.NewEncoder(w).Encode(result)
	})

	http.HandleFunc("/api/save", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
			return
		}

		fileName := strings.TrimSpace(r.URL.Query().Get("file"))
		if fileName == "" {
			http.Error(w, "Missing file parameter", http.StatusBadRequest)
			return
		}

		// Normalize and validate path
		cleanPath := filepath.Clean(filepath.FromSlash(fileName))

		// Security: Only allow relative paths inside workspace root and requiring .json extension
		if filepath.IsAbs(cleanPath) ||
			cleanPath == ".." ||
			strings.HasPrefix(cleanPath, ".."+string(filepath.Separator)) ||
			filepath.Ext(cleanPath) != ".json" {
			http.Error(w, "Invalid file path (must be a relative .json path within workspace)", http.StatusBadRequest)
			return
		}

		body, err := io.ReadAll(r.Body)
		if err != nil {
			http.Error(w, "Failed to read body", http.StatusInternalServerError)
			return
		}

		// Ensure parent directory exists
		parentDir := filepath.Dir(cleanPath)
		if parentDir != "." && parentDir != "" {
			if err := os.MkdirAll(parentDir, 0755); err != nil {
				http.Error(w, "Failed to create directory: "+err.Error(), http.StatusInternalServerError)
				return
			}
		}

		err = os.WriteFile(cleanPath, body, 0644)
		if err != nil {
			http.Error(w, "Failed to write file: "+err.Error(), http.StatusInternalServerError)
			return
		}

		fmt.Printf("✅ Saved %s (%d bytes)\n", cleanPath, len(body))
		w.WriteHeader(http.StatusOK)
		w.Write([]byte("OK"))
	})

	log.Fatal(http.ListenAndServe(":"+port, nil))
}

func openBrowser(url string) {
	var err error
	switch runtime.GOOS {
	case "windows":
		err = exec.Command("cmd", "/c", "start", url).Start()
	case "darwin":
		err = exec.Command("open", url).Start()
	default:
		err = exec.Command("xdg-open", url).Start()
	}
	if err != nil {
		log.Printf("Не удалось автоматически открыть браузер: %v\n", err)
	}
}
