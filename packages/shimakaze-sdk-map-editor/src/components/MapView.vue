<template>
  <div
    ref="root"
    class="border h-full"
  />
</template>

<script lang="ts" scoped setup>
import { onMounted, ref } from 'vue'
import * as THREE from 'three'

const root = ref<HTMLDivElement>()
let scene: THREE.Scene,
  camera: THREE.Camera,
  renderer: THREE.WebGLRenderer

// let geometry: THREE.BufferGeometry,
//   material: THREE.MeshBasicMaterial,
//   cube: THREE.Mesh

const init3D = (width: number, height: number) => {
  scene = new THREE.Scene()
  // camera = new THREE.OrthographicCamera(0, 100, 0, 100, 0.1, 1000)
  camera = new THREE.PerspectiveCamera(50, width / height, 0.1, 1000)
  renderer = new THREE.WebGLRenderer()
  renderer.setSize(width, height)
  root.value?.appendChild(renderer.domElement)

  const grid = new THREE.GridHelper(100, 10, 0x2C2C2C, 0x888888)
  const axes = new THREE.AxesHelper(1000)
  scene.add(grid)
  scene.add(axes)

  grid.rotation.x = Math.PI / 2
  axes.rotation.x = Math.PI

  camera.position.z = 150
  camera.lookAt(0, 0, 0)
}

const createTile = async (url: string) => {
  // const texture = await new THREE.TextureLoader().loadAsync(url)
  const geometry = new THREE.PlaneGeometry(12, 6, 1, 1)
  const material = new THREE.MeshBasicMaterial({
    // map: texture,
    // transparent: true,
    color: 0x333333,
    side: THREE.DoubleSide
  })
  const tile = new THREE.Mesh(geometry, material)

  // scene.add(cube)
  return tile
}

const setTilePosition = (tile: THREE.Mesh, x: number, y: number) => {
  tile.position.x = x * 12 + 6
  tile.position.y = -y * 6 - 3
}

const create = async () => {
  // const texture = await new THREE.TextureLoader().loadAsync('Shimakaze_Full.png')
  // // geometry = new THREE.PlaneGeometry(75, 132, 1, 1)
  // geometry = new THREE.PlaneGeometry(5.68, 10, 1, 1)
  // material = new THREE.MeshBasicMaterial({
  //   map: texture,
  //   transparent: true,
  //   // color: 0x333333,
  //   side: THREE.DoubleSide
  // })
  // cube = new THREE.Mesh(geometry, material)

  // cube.position.x += 5.68 / 2
  // cube.position.y -= 10 / 2
  // scene.add(cube)
  const tile1 = await createTile('')
  scene.add(tile1)
  setTilePosition(tile1, 0, 0)
  const tile2 = await createTile('')
  scene.add(tile2)
  setTilePosition(tile2, 1, 0)
  const tile3 = await createTile('')
  scene.add(tile3)
  setTilePosition(tile3, 0, 1)
}

const update = () => {
  // cube.rotation.x += 0.01
  // cube.rotation.y += 0.01
}

const renderLoop = () => {
  requestAnimationFrame(renderLoop)
  update()
  renderer.render(scene, camera)
}

const init = async () => {
  if (!root.value) return
  init3D(root.value.clientWidth, root.value.clientHeight)
  // init3D(1200, 600)
  await create()
  renderLoop()
}

onMounted(init)
</script>
