#pragma once

#include <windowsx.h>
#include "resource.h"

struct WindowData {
    bool isRed;   // цвет крестика для окна
    int  index;   // индекс 0/1
    POINT coords; // координаты
};