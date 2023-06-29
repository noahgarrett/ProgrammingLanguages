#ifndef clox_chunk_h
#define clox_chunk_h

#include "common.h"
#include "value.h"

typedef enum {
	OP_CONSTANT, // Ex. [00][23] <- [opcode][constant index] (2 bytes)
	OP_NIL,
	OP_TRUE,
	OP_FALSE,
	OP_POP,
	OP_GET_LOCAL,
	OP_SET_LOCAL,
	OP_GET_GLOBAL,
	OP_SET_GLOBAL,
	OP_DEFINE_GLOBAL,
	OP_GET_UPVALUE,
	OP_SET_UPVALUE,
	OP_GET_PROPERTY,
	OP_SET_PROPERTY,
	OP_GET_SUPER,
	OP_EQUAL,
	OP_GREATER,
	OP_LESS, // Need to add !=, <=, and >= later
	OP_ADD,
	OP_SUBTRACT,
	OP_MULTIPLY,
	OP_DIVIDE,
	OP_NOT,
	OP_NEGATE,
	OP_PRINT,
	OP_JUMP,
	OP_JUMP_IF_FALSE,
	OP_LOOP,
	OP_CALL,
	OP_INVOKE,
	OP_SUPER_INVOKE,
	OP_CLOSURE,
	OP_CLOSE_UPVALUE,
	OP_RETURN, // Ex. [01] <- opcode (1 byte)
	OP_CLASS,
	OP_INHERIT,
	OP_METHOD,
	
	// Lists
	OP_BUILD_LIST,
	OP_INDEX_SUBSCR,
	OP_STORE_SUBSCR
} OpCode;

typedef struct {
	int count; // The number of allocated elements that are actually in use
	int capacity; // The number of elements we have allocated
	uint8_t* code;
	int* lines; // Stores the chunk's line numbers
	ValueArray constants; // Stores the chunk's constants
} Chunk;

// Initializes a new chunk
void initChunk(Chunk* chunk);

// Frees a chunk in memory
void freeChunk(Chunk* chunk);

// Append a byte to the end of a chunk
void writeChunk(Chunk* chunk, uint8_t byte, int line);

// Adds a constant to the chunk
// Returns: The index where the constant was appended
int addConstant(Chunk* chunk, Value value);

#endif // !clox_chunk_h

