#pragma once

#include <windowsx.h>
#include "resource.h"
#include <cmath>

struct WindowData {
    bool isRed;   // ���� �������� ��� ����
    int  index;   // ������ 0/1
    POINT coords; // ����������
};