#pragma once
#include <windows.h>
#include <d3d9.h>

class CRender
{
public:
	virtual ~CRender();

	HRESULT CheckDeviceState();
	HRESULT CreateSurface(UINT uWidth, UINT uHeight, bool fUseAlpha, UINT m_uNumSample);

	virtual HRESULT Render() = 0;

	IDirect3DSurface9* GetSurfaceNoRef() { return m_pd3dRTS; }

protected:
	CRender();

	virtual HRESULT Init(IDirect3D9* p3D, IDirect3D9Ex* p3DEx, HWND hWnd, UINT uAdapter);

	IDirect3DDevice9* m_pd3dDevice;
	IDirect3DDevice9Ex* m_pd3dDeviceEx;

	IDirect3DSurface9* m_pd3dRTS;

};

