package main

import (
	"BlazeGo/repl"
	"fmt"
	"os"
	"os/user"
)

func main() {
	user, err := user.Current()
	if err != nil {
		panic(err)
	}

	fmt.Printf("Hello %s! This is the Blaze Programming Language!\n", user.Username)

	repl.Start(os.Stdin, os.Stdout)
}
