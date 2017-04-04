#pragma once
#include "Renderer.h"
class CLineStripRender : public CRenderer
{
public:
	static HRESULT Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRenderer **ppRenderer);
	~CLineStripRender();

	HRESULT Render();

protected:
	HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter);

private:
	CLineStripRender();
	IDirect3DVertexBuffer9 *m_pd3dVB;
};

