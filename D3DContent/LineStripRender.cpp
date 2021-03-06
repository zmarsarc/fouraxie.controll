#include "stdafx.h"
#include "LineStripRender.h"
#include <vector>

CLineStripRender::CLineStripRender() : CRenderer(), m_pd3dVB(NULL)
{
}

CLineStripRender::~CLineStripRender()
{
	SAFE_RELEASE(m_pd3dVB);
}

HRESULT CLineStripRender::Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRenderer **ppRenderer)
{
	HRESULT hr = S_OK;

	CLineStripRender *pRenderer = new CLineStripRender();
	IFCOOM(pRenderer);

	IFC(pRenderer->Init(pD3D, pD3DEx, hwnd, uAdapter));

	*ppRenderer = pRenderer;
	pRenderer = NULL;

Cleanup:
	delete pRenderer;

	return hr;
}

HRESULT CLineStripRender::Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter)
{
	HRESULT hr = S_OK;
	D3DXMATRIXA16 matView, matProj;
	D3DXVECTOR3 vEyePt(0.0f, 0.0f, -5.0f);
	D3DXVECTOR3 vLookatPt(0.0f, 0.0f, 0.0f);
	D3DXVECTOR3 vUpVec(0.0f, 1.0f, 0.0f);

	curEyePoint = vEyePt;
	curLookAt = vLookatPt;
	curUpVec = vUpVec;
	lookDirection = {0.0f, 0.0f, 1.0f};

	// Call base to create the device and render target
	IFC(CRenderer::Init(pD3D, pD3DEx, hwnd, uAdapter));

	// Set up the camera
	D3DXMatrixLookAtLH(&matView, &vEyePt, &vLookatPt, &vUpVec);
	IFC(m_pd3dDevice->SetTransform(D3DTS_VIEW, &matView));
	D3DXMatrixPerspectiveFovLH(&matProj, D3DX_PI / 4, 1.0f, 1.0f, 100.0f);
	IFC(m_pd3dDevice->SetTransform(D3DTS_PROJECTION, &matProj));

	// Set up the global state
	IFC(m_pd3dDevice->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE));
	IFC(m_pd3dDevice->SetRenderState(D3DRS_LIGHTING, FALSE));
	IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX));

Cleanup:
	return hr;
}

HRESULT CLineStripRender::SetupBuffer()
{
	HRESULT hr = S_OK;

	// buffer must have elements
	if (vertex.empty()) {
		hr = E_UNEXPECTED;
		goto Cleanup;
	}

	UINT bufferSize = sizeof(CUSTOMVERTEX) * vertex.size();

	if (m_pd3dVB) {
		m_pd3dVB->Release();
		m_pd3dVB = nullptr;
	}

	IFC(m_pd3dDevice->CreateVertexBuffer(bufferSize, 0, D3DFVF_CUSTOMVERTEX, D3DPOOL_DEFAULT, &m_pd3dVB, NULL));
	IFC(FillBuffer(bufferSize, vertex.data()));
	IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX)));

	vertex.clear();  // auto clear buffer

Cleanup:
	return hr;
}

HRESULT CLineStripRender::FillBuffer(UINT bufferSize, const void* src) {
	HRESULT hr = S_OK;

	void *pVertices;
	IFC(m_pd3dVB->Lock(0, bufferSize, &pVertices, 0));
	memcpy(pVertices, src, bufferSize);
	m_pd3dVB->Unlock();

Cleanup:
	return hr;
}

HRESULT CLineStripRender::AddPoint(float x, float y, float z, DWORD color) {
	HRESULT hr = S_OK;
	vertex.push_back(CUSTOMVERTEX(x, y, z, color));
	return hr;
}

HRESULT CLineStripRender::CameraMoveTo(D3DXVECTOR3 des) {

	HRESULT hr = S_OK;
	D3DXMATRIXA16 matView;
	D3DXVECTOR3 vLookatPt = des + lookDirection;

	D3DXMatrixLookAtLH(&matView, &des, &vLookatPt, &curUpVec);
	IFC(m_pd3dDevice->SetTransform(D3DTS_VIEW, &matView));

	curEyePoint = des;
	curLookAt = vLookatPt;

Cleanup:
	return hr;
}

HRESULT CLineStripRender::CameraLookAt(D3DXVECTOR3 des) {
	
	HRESULT hr = S_OK;
	D3DXMATRIXA16 matView;

	D3DXMatrixLookAtLH(&matView, &curEyePoint, &des, &curUpVec);
	IFC(m_pd3dDevice->SetTransform(D3DTS_VIEW, &matView));

	curLookAt = des;
	lookDirection = des - curEyePoint;
	lookDirection /= sqrtf(
		lookDirection.x * lookDirection.x + lookDirection.y * lookDirection.y + lookDirection.z * lookDirection.z
	);

Cleanup:
	return hr;
}

HRESULT CLineStripRender::CameraMove(D3DXVECTOR3 dir) {

	HRESULT hr = S_OK;
	D3DXMATRIXA16 matView;
	D3DXVECTOR3 vEyePoint = curEyePoint + dir;
	D3DXVECTOR3 vLookatPt = vEyePoint + lookDirection;

	D3DXMatrixLookAtLH(&matView, &vEyePoint, &vLookatPt, &curUpVec);
	IFC(m_pd3dDevice->SetTransform(D3DTS_VIEW, &matView));

	curEyePoint = vEyePoint;
	curLookAt = vLookatPt;

Cleanup:
	return hr;
}

HRESULT CLineStripRender::CameraRotate(D3DXVECTOR3 rad) {

	HRESULT hr = S_OK;
	D3DXMATRIXA16 matView;
	D3DXVECTOR3 dir = lookDirection;

	// rotate x
	dir.y = dir.y * cos(rad.x) - dir.z * sin(rad.x);
	dir.z = dir.y * sin(rad.x) + dir.z * cos(rad.x);

	// rotate y
	dir.x = dir.x * cos(rad.y) + dir.z * sin(rad.y);
	dir.z = -dir.x * sin(rad.y) + dir.z * cos(rad.y);

	// rotate z
	dir.x = dir.x * cos(rad.z) - dir.y * sin(rad.z);
	dir.y = dir.x * sin(rad.z) + dir.y * cos(rad.z);

	D3DXVECTOR3 vLookatPt = curEyePoint + dir;

	D3DXMatrixLookAtLH(&matView, &curEyePoint, &vLookatPt, &curUpVec);
	IFC(m_pd3dDevice->SetTransform(D3DTS_VIEW, &matView));

	curLookAt = vLookatPt;
	lookDirection = dir;

Cleanup:
	return hr;
}

HRESULT CLineStripRender::Draw(D3DXMATRIXA16* matWorld, size_t count) {
	HRESULT hr = S_OK;

	IFC(m_pd3dDevice->BeginScene());
	IFC(m_pd3dDevice->Clear(
		0,
		NULL,
		D3DCLEAR_TARGET,
		D3DCOLOR_ARGB(0, 0, 0, 0),
		1.0f,
		0
	));

	D3DXMatrixTranslation(matWorld, 0.0f, 0.0f, 0.0f);
	IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, matWorld));

	IFC(m_pd3dDevice->DrawPrimitive(D3DPT_LINESTRIP, 0, count - 1));

	IFC(m_pd3dDevice->EndScene());

Cleanup:
	return hr;
}

HRESULT CLineStripRender::Render()
{
	HRESULT hr = S_OK;
	D3DXMATRIXA16 matWorld;

	auto countPoint = vertex.size();
	static bool isInit = false;
	// just run once
	if (!isInit) {
		IFC(SetupBuffer());
		isInit = true;
	}
	vertex.clear();
	D3DXMatrixTranslation(&matWorld, 0.0f, 0.0f, 0.0f);

	IFC(Draw(&matWorld, countPoint));

Cleanup:
	return hr;
}