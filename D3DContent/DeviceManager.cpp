#include "stdafx.h"

const static TCHAR szAppName[] = TEXT("HostedRenderer");

DeviceManager::DeviceManager()
	:
	d3d(NULL),
	d3dEx(NULL),
	m_cAdapters(0),
	hWnd(NULL),
	m_pCurrentRenderer(NULL),
	m_rgRenderers(NULL),
	m_uWidth(1024),
	m_uHeight(1024),
	m_uNumSamples(0),
	m_fUseAlpha(false),
	m_fSurfaceSettingsChanged(true),
	deviceEx(NULL)
{
}

DeviceManager::~DeviceManager()
{
	DestroyResources();

	if (hWnd)
	{
		DestroyWindow(hWnd);
		UnregisterClass(szAppName, NULL);
	}
}

DeviceManager& DeviceManager::GetManager() {
	static DeviceManager deviceManager;
	return deviceManager;
}

void DeviceManager::Release() {
	this->~DeviceManager();
}

// Renderer's creation should controlled by Renderer itself
// This method will moved into Renderer later
HRESULT DeviceManager::EnsureRenderers() {
	HRESULT hr = S_OK;

	if (!m_rgRenderers)
	{
		IFC(CreateHWND());

		assert(m_cAdapters);
		m_rgRenderers = new CRenderer*[m_cAdapters];
		IFCOOM(m_rgRenderers);
		ZeroMemory(m_rgRenderers, m_cAdapters * sizeof(m_rgRenderers[0]));

		for (UINT i = 0; i < m_cAdapters; ++i)
		{
			IFC(CLineStripRender::Create(d3d, d3dEx, hWnd, i, &m_rgRenderers[i]));
		}

		// Default to the default adapter 
		m_pCurrentRenderer = m_rgRenderers[0];
	}

Cleanup:
	return hr;
}

HRESULT DeviceManager::CreateHWND() {

	HRESULT hr = S_OK;

	if (!hWnd)
	{
		WNDCLASS wndclass;

		wndclass.style = CS_HREDRAW | CS_VREDRAW;
		wndclass.lpfnWndProc = DefWindowProc;
		wndclass.cbClsExtra = 0;
		wndclass.cbWndExtra = 0;
		wndclass.hInstance = NULL;
		wndclass.hIcon = LoadIcon(NULL, IDI_APPLICATION);
		wndclass.hCursor = LoadCursor(NULL, IDC_ARROW);
		wndclass.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);
		wndclass.lpszMenuName = NULL;
		wndclass.lpszClassName = szAppName;

		if (!RegisterClass(&wndclass))
		{
			IFC(E_FAIL);
		}

		hWnd = CreateWindow(szAppName,
			TEXT("D3DImageSample"),
			WS_OVERLAPPEDWINDOW,
			0,                   // Initial X
			0,                   // Initial Y
			0,                   // Width
			0,                   // Height
			NULL,
			NULL,
			NULL,
			NULL);
	}

Cleanup:
	return hr;
}

DeviceManager::DIRECT3DCREATE9EXFUNCTION DeviceManager::GetExCreatorAddress() {
	HMODULE hD3D = LoadLibrary(TEXT("d3d9.dll"));
	DIRECT3DCREATE9EXFUNCTION pfnCreate9Ex = (DIRECT3DCREATE9EXFUNCTION)GetProcAddress(hD3D, "Direct3DCreate9Ex");
	FreeLibrary(hD3D);
	return pfnCreate9Ex;
}

HRESULT DeviceManager::CreateD3D9ExInstance(DIRECT3DCREATE9EXFUNCTION creator) {
	HRESULT hr = S_OK;

	IFC((*creator)(D3D_SDK_VERSION, &d3dEx));
	IFC(d3dEx->QueryInterface(__uuidof(IDirect3D9), reinterpret_cast<void **>(&d3d)));

Cleanup:
	return hr;
}

HRESULT DeviceManager::EnsureD3DObjects() {

	HRESULT hr = S_OK;
	if (!d3d) {
		DIRECT3DCREATE9EXFUNCTION pfnCreate9Ex = GetExCreatorAddress();
		if (pfnCreate9Ex) {
			IFC(CreateD3D9ExInstance(pfnCreate9Ex));
		}
		else {
			d3d = Direct3DCreate9(D3D_SDK_VERSION);
			if (!d3d) {
				IFC(E_FAIL);
			}
		}
		m_cAdapters = d3d->GetAdapterCount();
	}

Cleanup:
	return hr;
}

void DeviceManager::CleanupInvalidDevices()
{
	for (UINT i = 0; i < m_cAdapters; ++i)
	{
		if (FAILED(m_rgRenderers[i]->CheckDeviceState()))
		{
			DestroyResources();
			break;
		}
	}
}

HRESULT DeviceManager::GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
	HRESULT hr = S_OK;

	// Make sure we at least return NULL
	*ppSurface = NULL;

	CleanupInvalidDevices();

	// *** still we keep EnsureD3DObjects here
	// because GetBackBufferNoRef may called before DeviceManager init
	IFC(EnsureD3DObjects());

	//
	// Even if we never render to another adapter, this sample creates devices
	// and resources on each one. This is a potential waste of video memory,
	// but it guarantees that we won't have any problems (e.g. out of video
	// memory) when switching to render on another adapter. In your own code
	// you may choose to delay creation but you'll need to handle the issues
	// that come with it.
	//

	IFC(EnsureRenderers());

	if (m_fSurfaceSettingsChanged)
	{
		if (FAILED(TestSurfaceSettings()))
		{
			IFC(E_FAIL);
		}

		for (UINT i = 0; i < m_cAdapters; ++i)
		{
			IFC(m_rgRenderers[i]->CreateSurface(m_uWidth, m_uHeight, m_fUseAlpha, m_uNumSamples));
		}

		m_fSurfaceSettingsChanged = false;
	}

	if (m_pCurrentRenderer)
	{
		*ppSurface = m_pCurrentRenderer->GetSurfaceNoRef();
	}

Cleanup:
	// If we failed because of a bad device, ignore the failure for now and 
	// we'll clean up and try again next time.
	if (hr == D3DERR_DEVICELOST)
	{
		hr = S_OK;
	}

	return hr;
}

HRESULT DeviceManager::TestSurfaceSettings()
{
	HRESULT hr = S_OK;

	D3DFORMAT fmt = m_fUseAlpha ? D3DFMT_A8R8G8B8 : D3DFMT_X8R8G8B8;

	// 
	// We test all adapters because because we potentially use all adapters.
	// But even if this sample only rendered to the default adapter, you
	// should check all adapters because WPF may move your surface to
	// another adapter for you!
	//

	for (UINT i = 0; i < m_cAdapters; ++i)
	{
		// Can we get HW rendering?
		IFC(d3d->CheckDeviceType(
			i,
			D3DDEVTYPE_HAL,
			D3DFMT_X8R8G8B8,
			fmt,
			TRUE
		));

		// Is the format okay?
		IFC(d3d->CheckDeviceFormat(
			i,
			D3DDEVTYPE_HAL,
			D3DFMT_X8R8G8B8,
			D3DUSAGE_RENDERTARGET | D3DUSAGE_DYNAMIC, // We'll use dynamic when on XP
			D3DRTYPE_SURFACE,
			fmt
		));

		// D3DImage only allows multisampling on 9Ex devices. If we can't 
		// multisample, overwrite the desired number of samples with 0.
		if (d3dEx && m_uNumSamples > 1)
		{
			assert(m_uNumSamples <= 16);

			if (FAILED(d3d->CheckDeviceMultiSampleType(
				i,
				D3DDEVTYPE_HAL,
				fmt,
				TRUE,
				static_cast<D3DMULTISAMPLE_TYPE>(m_uNumSamples),
				NULL
			)))
			{
				m_uNumSamples = 0;
			}
		}
		else
		{
			m_uNumSamples = 0;
		}
	}

Cleanup:
	return hr;
}

void DeviceManager::DestroyResources()
{
	SAFE_RELEASE(d3d);
	SAFE_RELEASE(d3dEx);

	for (UINT i = 0; i < m_cAdapters; ++i)
	{
		delete m_rgRenderers[i];
	}
	delete[] m_rgRenderers;
	m_rgRenderers = NULL;

	m_pCurrentRenderer = NULL;
	m_cAdapters = 0;

	m_fSurfaceSettingsChanged = true;
}

static D3DPRESENT_PARAMETERS* SetupPrensentParamenters() {
	D3DPRESENT_PARAMETERS* ret = new D3DPRESENT_PARAMETERS;
	ZeroMemory(ret, sizeof(D3DPRESENT_PARAMETERS));
	ret->Windowed = true;
	ret->BackBufferFormat = D3DFORMAT::D3DFMT_UNKNOWN;
	ret->BackBufferHeight = 1;
	ret->BackBufferWidth = 1;
	ret->SwapEffect = D3DSWAPEFFECT::D3DSWAPEFFECT_DISCARD;
	return ret;
}

void DeviceManager::InitDevice() {

	D3DPRESENT_PARAMETERS* d3dpp = SetupPrensentParamenters();	
	DWORD dwVertexProcessing = D3DCREATE_HARDWARE_VERTEXPROCESSING;

	d3dEx->CreateDeviceEx(
		D3DADAPTER_DEFAULT,
		D3DDEVTYPE_HAL,
		hWnd,
		dwVertexProcessing | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE,
		d3dpp,
		NULL,
		&deviceEx
	);

	delete d3dpp;
}

void DeviceManager::SetSize(UINT uWidth, UINT uHeight)
{
	if (uWidth != m_uWidth || uHeight != m_uHeight)
	{
		m_uWidth = uWidth;
		m_uHeight = uHeight;
		m_fSurfaceSettingsChanged = true;
	}
}

void DeviceManager::SetAlpha(bool fUseAlpha)
{
	if (fUseAlpha != m_fUseAlpha)
	{
		m_fUseAlpha = fUseAlpha;
		m_fSurfaceSettingsChanged = true;
	}
}

void DeviceManager::SetNumDesiredSamples(UINT uNumSamples)
{
	if (m_uNumSamples != uNumSamples)
	{
		m_uNumSamples = uNumSamples;
		m_fSurfaceSettingsChanged = true;
	}
}

void DeviceManager::SetAdapter(POINT screenSpacePoint)
{
	CleanupInvalidDevices();

	//
	// After CleanupInvalidDevices, we may not have any D3D objects. Rather than
	// recreate them here, ignore the adapter update and wait for render to recreate.
	//

	if (d3d && m_rgRenderers)
	{
		HMONITOR hMon = MonitorFromPoint(screenSpacePoint, MONITOR_DEFAULTTONULL);

		for (UINT i = 0; i < m_cAdapters; ++i)
		{
			if (hMon == d3d->GetAdapterMonitor(i))
			{
				m_pCurrentRenderer = m_rgRenderers[i];
				break;
			}
		}
	}
}

IDirect3DDevice9Ex& DeviceManager::GetDeviceEx()
{
	return *deviceEx;
}

HRESULT DeviceManager::GetCurrentRenderer(CRenderer** pRenderer) {
	HRESULT hr = S_OK;

	if (!m_pCurrentRenderer) {
		hr = E_FAIL;
	}
	else {
		*pRenderer = m_pCurrentRenderer;
	}

	return hr;
}

HRESULT DeviceManager::Render()
{
	return m_pCurrentRenderer ? m_pCurrentRenderer->Render() : S_OK;
}

IDirect3D9* DeviceManager::GetD3D() {
	EnsureD3DObjects();
	return d3d;
}

IDirect3D9Ex* DeviceManager::GetD3DEx() {
	EnsureD3DObjects();
	return d3dEx;
}