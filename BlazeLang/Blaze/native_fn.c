#include <time.h>

#include "vm.h"

/*
	Used for calculating execution times
*/
static Value clockNative(int argCount, Value* args) {
	return NUMBER_VAL((double)clock() / CLOCKS_PER_SEC);
}

#pragma region Lists
static Value appendListNative(int argCount, Value* args) {
	// Append a value to the end of a list increasing the list's length by 1
	if (argCount != 2 || !IS_LIST(args[0])) {
		// Handle Error
	}

	ObjList* list = AS_LIST(args[0]);
	Value item = args[1];

	appendToList(list, item);
	return NIL_VAL;
}

static Value removeIndexListNative(int argCount, Value* args) {
	// Delete an item form a list at the given index
	if (argCount != 2 || !IS_LIST(args[0]) || !IS_NUMBER(args[1])) {
		// Handle Error
	}

	ObjList* list = AS_LIST(args[0]);
	int index = AS_NUMBER(args[1]);

	if (!isValidListIndex(list, index)) {
		// Handle Error
	}

	deleteFromList(list, index);
	return NIL_VAL;
}

static Value sliceListNative(int argCount, Value* args) {
	// Slice the list to form a new list between the specified indexes
	if (argCount < 3 || argCount > 4) {
		// Handle Error
	}

	ObjList* list = AS_LIST(args[0]);
	int start = AS_NUMBER(args[1]);
	int end = AS_NUMBER(args[2]);
	int step = argCount == 4 ? AS_NUMBER(args[3]) : 1;

	ObjList* slicedList = newList();

	// Copy elements from original array to the slice with the step
	for (int i = start; i < end; i += step) {
		appendToList(slicedList, list->items[i]);
	}

	return OBJ_VAL(slicedList);
}
#pragma endregion

void initNatives() {
	defineNative("clock", clockNative);
	defineNative("append", appendListNative);
	defineNative("remove", removeIndexListNative);
	defineNative("slice", sliceListNative);
}