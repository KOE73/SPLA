package main

import (
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

// noCacheForData makes the browser revalidate every model file it reads.
func noCacheForData(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if strings.HasSuffix(r.URL.Path, ".json") {
			w.Header().Set("Cache-Control", "no-cache")
		}
		next.ServeHTTP(w, r)
	})
}

func main() {
	port := "8777"
	url := fmt.Sprintf("http://localhost:%s/app/", port)

	fmt.Printf("🚀 Starting SPLA Visualizer server on %s\n", url)
	fmt.Println("Press Ctrl+C to stop.")

	go func() {
		time.Sleep(500 * time.Millisecond)
		openBrowser(url)
	}()

	// The model files are edited by hand, by the editor and by migration scripts,
	// often several times a minute. http.FileServer sends only Last-Modified, and
	// on that alone a browser is free to reuse a copy without asking — which is
	// how an editor session ends up loading a project as it was before a
	// migration and reporting it as broken. Data revalidates every time; the
	// hashed app bundle is immutable by name and does not need to.
	http.Handle("/", noCacheForData(http.FileServer(http.Dir("."))))
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
