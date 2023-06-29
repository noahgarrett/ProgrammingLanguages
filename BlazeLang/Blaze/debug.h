#ifndef clox_debug_h
#define clox_debug_h

#include "chunk.h"

// Disassembles all of the instructions in the chunk
void disassembleChunk(Chunk* chunk, const char* name);

// Disassembles an instruction located within the chunk
int disassembleInstruction(Chunk* chunk, int offset);

#endif // !clox_debug_h
