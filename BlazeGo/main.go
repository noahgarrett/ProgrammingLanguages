package main

import (
	"BlazeGo/compiler"
	"BlazeGo/lexer"
	"BlazeGo/parser"
	"BlazeGo/repl"
	"BlazeGo/vm"
	"fmt"
	"io"
	"io/ioutil"
	"os"
	"os/user"
)

const USE_REPL = true

func main() {
	user, err := user.Current()
	if err != nil {
		panic(err)
	}

	if USE_REPL {
		fmt.Printf("Hello %s! This is the Blaze Programming Language!\n", user.Username)

		repl.Start(os.Stdin, os.Stdout)
	} else {
		filePath := "C:\\Users\\NoahGarrett\\Desktop\\Github\\Noah\\ProgrammingLanguages\\BlazeGo\\test.lime"

		content, err := ioutil.ReadFile(filePath)
		if err != nil {
			fmt.Printf("Error reading file: &v\n", err)
			return
		}

		fileContent := string(content)

		l := lexer.New(fileContent)
		p := parser.New(l)

		program := p.ParseProgram()
		if len(p.Errors()) != 0 {
			printParserErrors(os.Stdout, p.Errors())
			return
		}

		comp := compiler.New()
		err = comp.Compile(program)
		if err != nil {
			fmt.Fprintf(os.Stdout, "Woops! Compilation failed:\n %s\n", err)
			return
		}

		machine := vm.New(comp.Bytecode())
		err = machine.Run()
		if err != nil {
			fmt.Fprintf(os.Stdout, "Woops! Executing Bytecode failed:\n %s\n", err)
			return
		}

		lastPopped := machine.LastPoppedStackElem()
		io.WriteString(os.Stdout, lastPopped.Inspect())
		io.WriteString(os.Stdout, "\n")

	}

}

func printParserErrors(out io.Writer, errors []string) {
	io.WriteString(out, "Woops! We ran into some monkey business here!\n")
	io.WriteString(out, " parser errors:\n")
	for _, msg := range errors {
		io.WriteString(out, "\t"+msg+"\n")
	}
}
