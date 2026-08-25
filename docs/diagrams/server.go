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
	"time"
)

func main() {
	port := "8777"
	url := fmt.Sprintf("http://localhost:%s", port)

	fmt.Printf("🚀 Starting SPLA Visualizer server on %s\n", url)
	fmt.Println("Press Ctrl+C to stop.")

	go func() {
		time.Sleep(500 * time.Millisecond)
		openBrowser(url)
	}()

	http.Handle("/", http.FileServer(http.Dir(".")))
	http.HandleFunc("/api/save", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
			return
		}
		
		fileName := r.URL.Query().Get("file")
		if fileName == "" || fileName == "catalog.js" {
			http.Error(w, "Invalid file name", http.StatusBadRequest)
			return
		}

		// Security: Only allow json files in the current dir
		if filepath.Ext(fileName) != ".json" || filepath.Base(fileName) != fileName {
			http.Error(w, "Invalid file path", http.StatusBadRequest)
			return
		}

		body, err := io.ReadAll(r.Body)
		if err != nil {
			http.Error(w, "Failed to read body", http.StatusInternalServerError)
			return
		}

		err = os.WriteFile(fileName, body, 0644)
		if err != nil {
			http.Error(w, "Failed to write file", http.StatusInternalServerError)
			return
		}
		
		fmt.Printf("✅ Saved %s\n", fileName)
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
