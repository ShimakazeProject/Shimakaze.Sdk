import * as THREE from 'three'

const textureLoader = new THREE.TextureLoader()
const geometry = new THREE.PlaneGeometry(60, 30, 1, 1)


const _textureCache = new Map<string, THREE.Texture>()
const _materialCache = new Map<THREE.Texture, THREE.MeshBasicMaterial>()

export const createTextureFromUri = async (uri: string) => {
  if (_textureCache.has(uri)) {
    return _textureCache.get(uri)!
  }
  const texture = await textureLoader.loadAsync(uri)
  _textureCache.set(uri, texture)
  return texture
}

export const createMaterialByTexture = (texture: THREE.Texture) => {
    if (_materialCache.has(texture)) {
    return _materialCache.get(texture)!
  }
  const material = new THREE.MeshBasicMaterial({
    map: texture,
    transparent: true,
    // color: 0x333333,
    side: THREE.FrontSide
  })
  _materialCache.set(texture, material)
  return material
}

export const createTile = (material: THREE.MeshBasicMaterial) => {
  const tile = new THREE.Mesh(geometry, material)
  return tile
}