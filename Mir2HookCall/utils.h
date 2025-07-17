#pragma once

// Delphi字符串结构体定义
struct DelphiString {
    DWORD refCount;
    DWORD length;
    WCHAR data[32];
};

// Extract wide string from integer array to buffer
inline void ExtractWideString(int* data, int startIndex, int length, wchar_t* buffer, int bufferSize) {
    // Prevent buffer overflow
    if (length > bufferSize - 1) length = bufferSize - 1;
    
    // Extract characters
    for (int i = 0; i < length; i++) {
        buffer[i] = (wchar_t)data[startIndex + i];
    }
    
    // Ensure null termination
    buffer[length] = 0;
}

// Extract string from data array and execute callback (simplified version)
template<typename Func>
inline void ProcessWideString(int* data, Func callback) {
    if (data && data[0] > 0) {
        int length = (int)data[0];
        if (length > 31) length = 31; // 防止缓冲区溢出
        
        wchar_t buffer[32] = {0};
        
        // 提取字符 (固定从data[2]开始)
        for (int i = 0; i < length; i++) {
            buffer[i] = (wchar_t)data[1 + i];
        }
        
        // 确保字符串以null结尾
        buffer[length] = 0;
        
        // 执行回调函数
        callback(buffer, length);
    }
}

// Extract multiple wide strings with lengths from integer array and execute callback
template<typename Func>
inline void ProcessWideStringsWithLengths(int* data, int count, Func callback) {
    if (!data) return;
    
    // Check if all lengths are valid
    bool allValid = true;
    for (int i = 0; i < count; i++) {
        if (data[i] <= 0) {
            allValid = false;
            break;
        }
    }
    
    if (!allValid) return;
    
    // Extract all strings
    wchar_t** strings = new wchar_t*[count];
    int* lengths = new int[count];
    int offset = count;
    
    for (int i = 0; i < count; i++) {
        int length = data[i];
        if (length > 31) length = 31; // Prevent buffer overflow
        
        strings[i] = new wchar_t[32];
        memset(strings[i], 0, 32 * sizeof(wchar_t));
        
        // Extract characters
        for (int j = 0; j < length; j++) {
            strings[i][j] = (wchar_t)data[offset + j];
        }
        
        // Update offset
        offset += length;
        
        // Save actual length
        lengths[i] = length;
    }
    
    // Execute callback
    callback(strings, lengths);
    
    // Free memory
    for (int i = 0; i < count; i++) {
        delete[] strings[i];
    }
    delete[] strings;
    delete[] lengths;
}

inline DelphiString* CreateDelphiString(const wchar_t* str, int length) {
    // Ensure length doesn't exceed buffer size
    if (length > 31) length = 31;
    
    DelphiString* result = new DelphiString();  // Use simple allocation
    result->refCount = -1;
    result->length = length;
    memcpy(result->data, str, length * sizeof(wchar_t));
    result->data[length] = 0; // Null terminate
    return result;
}


inline wchar_t* GetDelphiStringData(DelphiString* str) {
    return str->data;
}