#pragma once

#include <d3d9.h>

class CRenderer
{
public:
	virtual ~CRenderer();

	HRESULT CheckDeviceState();
	HRESULT CreateSurface(UINT uWidth, UINT uHeight, bool fUseAlpha, UINT m_uNumSamples);

	virtual HRESULT Render() = 0;
	virtual HRESULT AddPoint(float x, float y, float z, DWORD color) = 0;
	IDirect3DSurface9 *GetSurfaceNoRef() { return m_pd3dRTS; }
	
	// Camera Control
	virtual HRESULT CameraMoveTo(D3DXVECTOR3 des) = 0;
	virtual HRESULT CameraLookAt(D3DXVECTOR3 des) = 0;
	virtual HRESULT CameraMove(D3DXVECTOR3 dir) = 0;
	virtual HRESULT CameraRotate(D3DXVECTOR3 rad) = 0;

protected:
	CRenderer();

	virtual HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter);

	HRESULT CreateDeviceEx(IDirect3D9Ex* d3dEx, UINT adapter, HWND hWnd, DWORD vertexProcess, D3DPRESENT_PARAMETERS* pp);
	HRESULT CreateDevice(IDirect3D9* d3d, UINT adapter, HWND hWnd, DWORD vertexProcess, D3DPRESENT_PARAMETERS* pp);
	IDirect3DDevice9   *m_pd3dDevice;
	IDirect3DDevice9Ex *m_pd3dDeviceEx;

	IDirect3DSurface9 *m_pd3dRTS;

};