#ifndef clox_vm_h
#define clox_vm_h

#include "object.h"
#include "table.h"
#include "value.h"

#define FRAMES_MAX 64
#define STACK_MAX (FRAMES_MAX * UINT8_COUNT)

// Represents a single ongoing function call
typedef struct {
	ObjClosure* closure;
	uint8_t* ip;
	Value* slots; // Points to the VM's value stack at the first slot this function can use
} CallFrame;

typedef struct {
	CallFrame frames[FRAMES_MAX];
	int frameCount;

	Value stack[STACK_MAX];
	Value* stackTop; // Points just past the last item in the stack

	Table globals; // Hash table of globals
	Table strings; // Hash table of strings

	ObjString* initString;

	ObjUpvalue* openUpvalues;

	size_t bytesAllocated;
	size_t nextGC;

	Obj* objects; // Stores a pointer to the head of the linked list of objects in memory

	// Garbage Collection
	int grayCount;
	int grayCapacity;
	Obj** grayStack;
} VM;

typedef enum {
	INTERPRET_OK,
	INTERPRET_COMPILE_ERROR,
	INTERPRET_RUNTIME_ERROR
} InterpretResult;

// Globally exposing the vm object defined in vm.c
extern VM vm;

// Initialize the virtual machine
void initVM();

// Free the virtual machine from memory
void freeVM();

// Interprets the chunk of bytecode
InterpretResult interpret(const char* source);

// Push a new value onto the stack
void push(Value value);

// Pop the most recently added value off the stack
Value pop();

// Defines a native funtion
void defineNative(const char* name, NativeFn function);

#endif // !clox_vm_h
