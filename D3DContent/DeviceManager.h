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
	HRESULT EnsureHWND();
	HRESULT EnsureD3DObjects();
	HRESULT TestSurfaceSettings();
	void DestroyResources();
	void InitDevice();

	IDirect3D9    *m_pD3D;
	IDirect3D9Ex  *m_pD3DEx;

	UINT m_cAdapters;
	CRenderer **m_rgRenderers;
	CRenderer *m_pCurrentRenderer;

	HWND m_hwnd;

	UINT m_uWidth;
	UINT m_uHeight;
	UINT m_uNumSamples;
	bool m_fUseAlpha;
	bool m_fSurfaceSettingsChanged;

	IDirect3DDevice9Ex* device;
};