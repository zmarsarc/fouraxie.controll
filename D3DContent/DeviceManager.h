#pragma once

#include <d3d9.h>

class CRenderer;

class DeviceManager
{
public:
	static DeviceManager& GetManager();
	void Release();

	void SetSize(UINT uWidth, UINT uHeight);
	void SetAlpha(bool fUseAlpha);
	void SetNumDesiredSamples(UINT uNumSamples);
	void SetAdapter(POINT screenSpacePoint);
	IDirect3DDevice9Ex& GetDeviceEx();


	HRESULT GetCurrentRenderer(CRenderer** pRenderer);
	HRESULT GetBackBufferNoRef(IDirect3DSurface9 **ppSurface);
	HRESULT Render();

private:
	DeviceManager();
	DeviceManager(DeviceManager const&);
	DeviceManager& operator = (DeviceManager const&);
	~DeviceManager();

	void CleanupInvalidDevices();
	HRESULT EnsureRenderers();
	HRESULT CreateHWND();
	HRESULT EnsureD3DObjects();
	HRESULT TestSurfaceSettings();
	void DestroyResources();
	void InitDevice();

	IDirect3D9    *d3d;
	IDirect3D9Ex  *d3dEx;

	UINT m_cAdapters;
	CRenderer **m_rgRenderers;
	CRenderer *m_pCurrentRenderer;

	HWND hWnd;

	UINT m_uWidth;
	UINT m_uHeight;
	UINT m_uNumSamples;
	bool m_fUseAlpha;
	bool m_fSurfaceSettingsChanged;

	IDirect3DDevice9Ex* deviceEx;
};