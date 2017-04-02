#pragma once

class CTriangleRender : public CRender
{
public:
	static HRESULT Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRender **ppRenderer);
	~CTriangleRender();

	HRESULT Render();

protected:
	HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter);

private:
	CTriangleRender();

	IDirect3DVertexBuffer9 *m_pd3dVB;
};