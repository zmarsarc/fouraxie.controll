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
	HRESULT CameraLookAt(D3DXVECTOR3 des);
	HRESULT CameraMove(D3DXVECTOR3 dir);
	HRESULT CameraRotate(D3DXVECTOR3 rad);

protected:
	HRESULT Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter);

private:
	IDirect3DVertexBuffer9 *m_pd3dVB;
	std::vector<struct CUSTOMVERTEX> vertex;

	D3DXVECTOR3 curEyePoint;
	D3DXVECTOR3 curLookAt;
	D3DXVECTOR3 curUpVec;
	D3DXVECTOR3 lookDirection;

	CLineStripRender();
	HRESULT SetupBuffer();
};

