#pragma once

#include <windowsx.h>
#include "resource.h"

struct WindowData {
    bool isRed;   // ���� �������� ��� ����
    int  index;   // ������ 0/1
    POINT coords; // ����������
};