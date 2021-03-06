#pragma once

#include <d3d9.h>

class CTriangleRenderer : public CRenderer
{
public:
	static HRESULT Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRenderer **ppRenderer);
	~CTriangleRenderer();

	HRESULT Render();
	HRESULT AddPoint(float x, float y, float z, DWORD color);
	HRESULT CameraMoveTo(D3DXVECTOR3 des);
	HRESULT CameraLookAt(D3DXVECTOR3 des);
	HRESULT CameraMove(D3DXVECTOR3 dir);
	HRESULT CameraRotate(D3DXVECTOR3 rad);

protected:
	HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter);

private:
	CTriangleRenderer();

	IDirect3DVertexBuffer9 *m_pd3dVB;
};