// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

static DeviceManager& deviceManager = DeviceManager::GetManager();

extern "C" HRESULT WINAPI SetSize(UINT uWidth, UINT uHeight)
{
	deviceManager.SetSize(uWidth, uHeight);
	return S_OK;
}

extern "C" HRESULT WINAPI SetAlpha(BOOL fUseAlpha)
{
	deviceManager.SetAlpha(!!fUseAlpha);
	return S_OK;
}

extern "C" HRESULT WINAPI SetNumDesiredSamples(UINT uNumSamples)
{
	deviceManager.SetNumDesiredSamples(uNumSamples);
	return S_OK;
}

extern "C" HRESULT WINAPI SetAdapter(POINT screenSpacePoint)
{
	deviceManager.SetAdapter(screenSpacePoint);
	return S_OK;
}

extern "C" HRESULT WINAPI GetBackBufferNoRef(IDirect3DSurface9 **ppSurface)
{
	HRESULT hr = S_OK;

	IFC(deviceManager.GetBackBufferNoRef(ppSurface));

Cleanup:
	return hr;
}

extern "C" HRESULT WINAPI AddPoint(float x, float y, float z, DWORD color)
{

	CRenderer* currentRenderer;
	deviceManager.GetCurrentRenderer(&currentRenderer);
	return currentRenderer->AddPoint(x, y, z, color);
}

extern "C" HRESULT WINAPI Render()
{
	return deviceManager.Render();
}

extern "C" void WINAPI Destroy()
{
	deviceManager.Release();
	return;
}

extern "C" HRESULT WINAPI CameraMoveTo(float x, float y, float z) {
	
	HRESULT hr = S_OK;
	D3DXVECTOR3 des = { x, y, z };
	CRenderer* currentRenderer;
	IFC(deviceManager.GetCurrentRenderer(&currentRenderer));
	IFC(currentRenderer->CameraMoveTo(des));

Cleanup:
	return hr;
}

extern "C" HRESULT WINAPI CameraLookAt(float x, float y, float z) {

	HRESULT hr = S_OK;
	D3DXVECTOR3 des = { x, y, z };
	CRenderer* currentRenderer;
	IFC(deviceManager.GetCurrentRenderer(&currentRenderer));
	IFC(currentRenderer->CameraLookAt(des));

Cleanup:
	return hr;
}

extern "C" HRESULT WINAPI CameraMove(float x, float y, float z) {

	HRESULT hr = S_OK;
	D3DXVECTOR3 des = { x, y, z };
	CRenderer* currentRenderer;
	IFC(deviceManager.GetCurrentRenderer(&currentRenderer));
	IFC(currentRenderer->CameraMove(des));

Cleanup:
	return hr;
}

extern "C" HRESULT WINAPI CameraRotate(float x, float y, float z) {

	HRESULT hr = S_OK;
	D3DXVECTOR3 dir = { x, y, z };
	CRenderer* currentRenderer;
	IFC(deviceManager.GetCurrentRenderer(&currentRenderer));
	IFC(currentRenderer->CameraRotate(dir));

Cleanup:
	return hr;
}