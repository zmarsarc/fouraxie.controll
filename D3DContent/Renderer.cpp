#include "stdafx.h"

//+-----------------------------------------------------------------------------
//
//  Member:
//      CRenderer ctor
//
//------------------------------------------------------------------------------
CRenderer::CRenderer() : m_pd3dDevice(NULL), m_pd3dRTS(NULL), m_pd3dDeviceEx(NULL)
{
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CRenderer dtor
//
//------------------------------------------------------------------------------
CRenderer::~CRenderer()
{
	SAFE_RELEASE(m_pd3dDevice);
	SAFE_RELEASE(m_pd3dDeviceEx);
	SAFE_RELEASE(m_pd3dRTS);
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CRenderer::CheckDeviceState
//
//  Synopsis:
//      Returns the status of the device. 9Ex devices are a special case because 
//      TestCooperativeLevel() has been deprecated in 9Ex.
//
//------------------------------------------------------------------------------
HRESULT
CRenderer::CheckDeviceState()
{
	if (m_pd3dDeviceEx)
	{
		return m_pd3dDeviceEx->CheckDeviceState(NULL);
	}
	else if (m_pd3dDevice)
	{
		return m_pd3dDevice->TestCooperativeLevel();
	}
	else
	{
		return D3DERR_DEVICELOST;
	}
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CRenderer::CreateSurface
//
//  Synopsis:
//      Creates and sets the render target
//
//------------------------------------------------------------------------------
HRESULT
CRenderer::CreateSurface(UINT uWidth, UINT uHeight, bool fUseAlpha, UINT m_uNumSamples)
{
	HRESULT hr = S_OK;

	SAFE_RELEASE(m_pd3dRTS);

	IFC(m_pd3dDevice->CreateRenderTarget(
		uWidth,
		uHeight,
		fUseAlpha ? D3DFMT_A8R8G8B8 : D3DFMT_X8R8G8B8,
		static_cast<D3DMULTISAMPLE_TYPE>(m_uNumSamples),
		0,
		m_pd3dDeviceEx ? FALSE : TRUE,  // Lockable RT required for good XP perf
		&m_pd3dRTS,
		NULL
	));

	IFC(m_pd3dDevice->SetRenderTarget(0, m_pd3dRTS));

Cleanup:
	return hr;
}

static D3DPRESENT_PARAMETERS* SetupPrensentParamenters()
{
	D3DPRESENT_PARAMETERS* ret = new D3DPRESENT_PARAMETERS;
	ZeroMemory(ret, sizeof(D3DPRESENT_PARAMETERS));
	ret->Windowed = true;
	ret->BackBufferFormat = D3DFORMAT::D3DFMT_UNKNOWN;
	ret->BackBufferHeight = 1;
	ret->BackBufferWidth = 1;
	ret->SwapEffect = D3DSWAPEFFECT::D3DSWAPEFFECT_DISCARD;
	return ret;
}

static DWORD DetectHardwareCapabilities(D3DCAPS9 caps) {
	if ((caps.DevCaps & D3DDEVCAPS_HWTRANSFORMANDLIGHT) == D3DDEVCAPS_HWTRANSFORMANDLIGHT) {
		return D3DCREATE_HARDWARE_VERTEXPROCESSING;
	}
	else {
		return D3DCREATE_SOFTWARE_VERTEXPROCESSING;
	}
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CRenderer::Init
//
//  Synopsis:
//      Creates the device
//
//------------------------------------------------------------------------------
HRESULT
CRenderer::Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter)
{
	HRESULT hr = S_OK;

	D3DPRESENT_PARAMETERS* pd3dpp = SetupPrensentParamenters();

	D3DCAPS9 caps;
	IFC(pD3D->GetDeviceCaps(uAdapter, D3DDEVTYPE_HAL, &caps));
	DWORD dwVertexProcessing = DetectHardwareCapabilities(caps);

	if (pD3DEx) {
		CreateDeviceEx(pD3DEx, uAdapter, hwnd, dwVertexProcessing, pd3dpp);
		//m_pd3dDeviceEx = (DeviceManager::GetManager()).GetDeviceEx();
	}
	else {
		CreateDevice(pD3D, uAdapter, hwnd, dwVertexProcessing, pd3dpp);
	}

Cleanup:
	delete pd3dpp;
	return hr;
}

HRESULT CRenderer::CreateDeviceEx(IDirect3D9Ex* d3dEx, UINT adapter, HWND hWnd, DWORD vertexProcess, D3DPRESENT_PARAMETERS* pp) {

	HRESULT hr = S_OK;

	IFC(d3dEx->CreateDeviceEx(
		adapter,
		D3DDEVTYPE_HAL,
		hWnd,
		vertexProcess | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE,
		pp,
		NULL,
		&m_pd3dDeviceEx
	));

	IFC(m_pd3dDeviceEx->QueryInterface(__uuidof(IDirect3DDevice9), reinterpret_cast<void**>(&m_pd3dDevice)));

Cleanup:
	return hr;
}

HRESULT CRenderer::CreateDevice(IDirect3D9* d3d, UINT adapter, HWND hWnd, DWORD vertexProcess, D3DPRESENT_PARAMETERS* pp) {
	HRESULT hr = S_OK;

	assert(d3d);

	IFC(d3d->CreateDevice(
		adapter,
		D3DDEVTYPE_HAL,
		hWnd,
		vertexProcess | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE,
		pp,
		&m_pd3dDevice
	));

Cleanup:
	return hr;
}