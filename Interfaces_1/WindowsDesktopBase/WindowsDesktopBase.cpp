#include "framework.h"
#include "WindowsDesktopBase.h"

#define MAX_LOADSTRING 100
#define TIMER_ID 1

HINSTANCE hInst;                                // текущий экземпляр
WCHAR szTitle1[MAX_LOADSTRING];                 // Текст строки заголовка 1 окна
WCHAR szTitle2[MAX_LOADSTRING];                 // Текст строки заголовка 2 окна
WCHAR szWindowClass[MAX_LOADSTRING];            // имя класса главного окна

HWND hWnds[2] = { nullptr, nullptr };
bool sync = false;

ATOM                 MyRegisterClass(HINSTANCE hInstance);
BOOL                 InitInstance(HINSTANCE, int);
LRESULT CALLBACK     WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK     About(HWND, UINT, WPARAM, LPARAM);
static inline double Deg2Rad(double deg);
static void          DrawHand(HDC hdc, POINT c, double angleRad, int length, int width, COLORREF color);
static void          DrawClock(HDC hdc, RECT &rc, bool isRed);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPWSTR lpCmdLine, _In_ int nCmdShow) {
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    // Инициализация глобальных строк
    LoadStringW(hInstance, IDS_APP_TITLE1, szTitle1, MAX_LOADSTRING);
    LoadStringW(hInstance, IDS_APP_TITLE2, szTitle2, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_WINDOWSDESKTOPBASE, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // Выполнить инициализацию приложения:
    if (!InitInstance(hInstance, nCmdShow)) {
        return FALSE;
    }

    HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WINDOWSDESKTOPBASE));

    MSG msg;

    // Цикл основного сообщения:
    while (GetMessage(&msg, nullptr, 0, 0)) {
        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg)) {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    return (int)msg.wParam;
}

ATOM MyRegisterClass(HINSTANCE hInstance) {
    WNDCLASSEXW wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = WndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINDOWSDESKTOPBASE));
    wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
    wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WINDOWSDESKTOPBASE);
    wcex.lpszClassName = szWindowClass;
    wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassExW(&wcex);
}

BOOL InitInstance(HINSTANCE hInstance, int nCmdShow) {
    hInst = hInstance;

    HWND hWnd1 = CreateWindowW(szWindowClass, szTitle1, WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, (LPVOID)0);

    if (!hWnd1) {
        return FALSE;
    }

    ShowWindow(hWnd1, nCmdShow);
    UpdateWindow(hWnd1);

    HWND hWnd2 = CreateWindowW(szWindowClass, szTitle2, WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, (LPVOID)1);

    if (!hWnd2) {
        return FALSE;
    }

    ShowWindow(hWnd2, nCmdShow);
    UpdateWindow(hWnd2);

    return TRUE;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) {
    switch (message) {
        case WM_CREATE: // Создание
        {
            CREATESTRUCT *pcs = (CREATESTRUCT *)lParam;
            int index = (int)(INT_PTR)pcs->lpCreateParams;

            WindowData *data = new WindowData();
            data->isRed = true;
            data->index = index;
            data->coords = { 0, 0 };

            hWnds[index] = hWnd;
            SetWindowLongPtr(hWnd, GWLP_USERDATA, (LONG_PTR)data);

            SetTimer(hWnd, TIMER_ID, 1000, nullptr);
        }
        break;
        case WM_TIMER: // Событие таймера
            if (wParam == TIMER_ID) {
                InvalidateRect(hWnd, nullptr, FALSE);
            }
            break;
        case WM_COMMAND: // Меню
        {
            int wmId = LOWORD(wParam);
            // Разобрать выбор в меню:
            switch (wmId) {
                case IDM_ABOUT:
                    DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
                    break;
                case IDM_EXIT:
                    DestroyWindow(hWnd);
                    break;
                case IDM_TOGGLE_SYNC:
                    sync = !sync;
                    InvalidateRect(hWnds[0], nullptr, FALSE);
                    InvalidateRect(hWnds[1], nullptr, FALSE);
                    break;
                default:
                    return DefWindowProc(hWnd, message, wParam, lParam);
            }
        }
        break;
        case WM_PAINT: // Отрисовка
        {
            WindowData *data = (WindowData *)GetWindowLongPtr(hWnd, GWLP_USERDATA);
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);

            // Очистка фона
            HBRUSH hBrush = (HBRUSH)GetStockObject(WHITE_BRUSH);
            FillRect(hdc, &ps.rcPaint, hBrush);

            // Часы
            RECT rc;
            GetClientRect(hWnd, &rc);
            DrawClock(hdc, rc, data->isRed);

            // Координаты
            WCHAR buf[128];
            wsprintfW(buf, L"X=%d, Y=%d, Sync: %s", data->coords.x, data->coords.y, sync ? L"ON" : L"OFF");
            TextOutW(hdc, 10, 10, buf, lstrlenW(buf));

            EndPaint(hWnd, &ps);
        }
        break;
        case WM_MOUSEMOVE: // Движение мыши
        {
            WindowData *data = (WindowData *)GetWindowLongPtr(hWnd, GWLP_USERDATA);
            int x = GET_X_LPARAM(lParam);
            int y = GET_Y_LPARAM(lParam);
            data->coords.x = x;
            data->coords.y = y;
            InvalidateRect(hWnd, nullptr, FALSE);

            if (sync) {
                HWND hWndOther = hWnds[1 - data->index];
                WindowData *other = (WindowData *)GetWindowLongPtr(hWndOther, GWLP_USERDATA);
                other->coords = data->coords;
                InvalidateRect(hWndOther, nullptr, FALSE);
            }
        }
        break;
        case WM_LBUTTONDOWN: // Левая кнопка мыши
        {
            WindowData *data = (WindowData *)GetWindowLongPtr(hWnd, GWLP_USERDATA);
            data->isRed = !data->isRed;
            InvalidateRect(hWnd, NULL, TRUE);
        }
        break;
        case WM_DESTROY: // Закрытие
        {
            KillTimer(hWnd, TIMER_ID);
            WindowData *data = (WindowData *)GetWindowLongPtr(hWnd, GWLP_USERDATA);
            delete data;
            PostQuitMessage(0);
        }
        break;
        default:
            return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam) {
    UNREFERENCED_PARAMETER(lParam);
    switch (message) {
        case WM_INITDIALOG:
            return (INT_PTR)TRUE;

        case WM_COMMAND:
            if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL) {
                EndDialog(hDlg, LOWORD(wParam));
                return (INT_PTR)TRUE;
            }
            break;
    }
    return (INT_PTR)FALSE;
}

// Градусы в радианы
static inline double Deg2Rad(double deg) { return deg * 3.14159265358979323846 / 180.0; }

// Рисование стрелки
static void DrawHand(HDC hdc, POINT c, double angleRad, int length, int width, COLORREF color) {
    // Создание репа
    HPEN hPen = CreatePen(PS_SOLID, width, color);
    HPEN hOld = (HPEN)SelectObject(hdc, hPen);

    // Вычисление координат
    int x = std::lround(c.x + length * std::sin(angleRad));
    int y = std::lround(c.y - length * std::cos(angleRad));

    // Рисование
    MoveToEx(hdc, c.x, c.y, nullptr);
    LineTo(hdc, x, y);

    // Удаление репа
    SelectObject(hdc, hOld);
    DeleteObject(hPen);
}

// Рисование часов
static void DrawClock(HDC hdc, RECT &rc, bool isRed) {
    // Сохранение состояния
    int saved = SaveDC(hdc);

    // Вычисления
    int w = rc.right - rc.left;
    int h = rc.bottom - rc.top;
    int r = (min(w, h) / 2) - 10;
    POINT c{ rc.left + w / 2, rc.top + h / 2 };
    
    if (r < 10) {
        RestoreDC(hdc, saved);
        return;
    }

    // Создание ресусов
    SYSTEMTIME st{};
    GetLocalTime(&st);

    COLORREF mainColor = isRed ? RGB(255, 0, 0) : RGB(0, 200, 0);
    HBRUSH hOldBrush = (HBRUSH)SelectObject(hdc, GetStockObject(NULL_BRUSH));
    HPEN   hCirclePen = CreatePen(PS_SOLID, 2, mainColor);
    HPEN   hOldPen = (HPEN)SelectObject(hdc, hCirclePen);

    // Круга
    Ellipse(hdc, c.x - r, c.y - r, c.x + r, c.y + r);

    // Засечки
    for (int i = 0; i < 12; ++i) {
        double a = Deg2Rad(i * 30.0);
        int r1 = r - 8;
        int r2 = r;
        int x1 = std::lround(c.x + r1 * std::sin(a));
        int y1 = std::lround(c.y - r1 * std::cos(a));
        int x2 = std::lround(c.x + r2 * std::sin(a));
        int y2 = std::lround(c.y - r2 * std::cos(a));
        MoveToEx(hdc, x1, y1, nullptr);
        LineTo(hdc, x2, y2);
    }

    // Стрелки
    double hourA = Deg2Rad(((st.wHour % 12) + st.wMinute / 60.0) * 30.0);
    DrawHand(hdc, c, hourA, (int)(r * 0.55), 6, mainColor);
    double minA = Deg2Rad((st.wMinute + st.wSecond / 60.0) * 6.0);
    DrawHand(hdc, c, minA, (int)(r * 0.75), 4, mainColor);
    double secA = Deg2Rad(st.wSecond * 6.0);
    DrawHand(hdc, c, secA, (int)(r * 0.85), 2, RGB(0, 0, 0));

    // Центр
    HBRUSH hDot = CreateSolidBrush(RGB(0, 0, 0));
    SelectObject(hdc, hDot);
    Ellipse(hdc, c.x - 3, c.y - 3, c.x + 3, c.y + 3);
    DeleteObject(hDot);

    // Восстановление
    SelectObject(hdc, hOldPen);
    DeleteObject(hCirclePen);
    SelectObject(hdc, hOldBrush);
    RestoreDC(hdc, saved);
}