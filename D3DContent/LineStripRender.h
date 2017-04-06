#pragma once

#include "Renderer.h"
#include <vector>

class CLineStripRender : public CRenderer
{
public:
	static HRESULT Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRenderer **ppRenderer);
	~CLineStripRender();

	HRESULT Render();
	HRESULT AddPoint(float x, float y, float z, DWORD color);
	HRESULT CameraMoveTo(D3DXVECTOR3 des);

protected:
	HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter);

private:
	IDirect3DVertexBuffer9 *m_pd3dVB;
	std::vector<struct CUSTOMVERTEX> vertex;

	CLineStripRender();
	HRESULT SetupBuffer();
};

