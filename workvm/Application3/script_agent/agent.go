package main

import (
    "fmt"
    "net/http"
    "strings"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"bytes"
)

func pwd() string {
	ex, err := os.Executable()
    if err != nil {
        panic(err)
    }
    exPath := filepath.Dir(ex)
    return exPath
}

func sayhelloName(w http.ResponseWriter, r *http.Request) {
    r.ParseForm()  // parse arguments, you have to call this by yourself
    fmt.Println(r.Form)  // print form information in server side
    fmt.Println("path", r.URL.Path)
    fmt.Println("scheme", r.URL.Scheme)
    fmt.Println(r.Form["url_long"])
    for k, v := range r.Form {
        fmt.Println("key:", k)
        fmt.Println("val:", strings.Join(v, ""))
    }
    fmt.Fprintf(w, "Hello World!") // send data to client side
}

func executeScript(w http.ResponseWriter, r *http.Request) {
    r.ParseForm()  // parse arguments, you have to call this by yourself
    log.Println(r.Form)  // print form information in server side
    log.Println("path", r.URL.Path)
    log.Println(r.Form["url_long"])
	scriptName := strings.Join(r.Form["script"], "") 
	scriptArgs := r.Form["args[]"]
	log.Println("scriptName", scriptName, "scriptArgs", scriptArgs)
	if scriptName != "" {
		path := pwd() + "/"+scriptName
		log.Print(path)
		var stderr bytes.Buffer
		cmd := exec.Command("/bin/sh", append([]string{path}, scriptArgs...)...)
		cmd.Stderr = &stderr
		out, err := cmd.Output()
		if err != nil {
			log.Print(err)
		}
		log.Printf("output is %s\n", out)
		log.Printf("stderr is %s\n", stderr.String())
		fmt.Fprintf(w, "output=%s\n\nstderr=%s\n", out, stderr.String()) // send data to client side
	} else {
		fmt.Fprintf(w, "No script specified.")
	}
	log.Println("==============================")
    log.Println("")
}


func main() {
	log.Print("Starting script agent...")
    http.HandleFunc("/", sayhelloName) // set router
    http.HandleFunc("/run", executeScript) // set router
    err := http.ListenAndServe(":9090", nil) // set listen port
    if err != nil {
        log.Fatal("ListenAndServe: ", err)
    }
}