<template>
  <div
    ref="root"
    class="border h-full"
  />
</template>

<script lang="ts" scoped setup>
import { onMounted, ref } from 'vue'
import * as THREE from 'three'
import { createMaterialByTexture, createTextureFromUri, createTile } from '../utils/createTile';

const root = ref<HTMLDivElement>()
let scene: THREE.Scene,
  camera: THREE.Camera,
  renderer: THREE.WebGLRenderer

// let geometry: THREE.BufferGeometry,
//   material: THREE.MeshBasicMaterial,
//   cube: THREE.Mesh

const initMouseEvent = () => {

  let mouse1down = false

  window.addEventListener('wheel', e => {
    camera.position.z += e.deltaY
  })
  window.addEventListener('mousedown', e => {
    if (e.button === 1) {
      mouse1down = true
    }
  })
  window.addEventListener('mouseup', e => {
    if (e.button === 1) {
      mouse1down = false
    }
  })
  window.addEventListener('mousemove', e => {
    if (mouse1down) {
      camera.position.x -= e.movementX
      camera.position.y += e.movementY
    }
  })
}

const init3D = (width: number, height: number) => {
  scene = new THREE.Scene()
  // camera = new THREE.OrthographicCamera(0, 100, 0, 100, 0.1, 1000)
  camera = new THREE.PerspectiveCamera(50, width / height, 0.1, 2000)
  renderer = new THREE.WebGLRenderer()
  renderer.setSize(width, height)
  root.value?.appendChild(renderer.domElement)

  // const grid = new THREE.GridHelper(1000, 30, 0x2C2C2C, 0x888888)
  const axes = new THREE.AxesHelper(1000)
  // scene.add(grid)
  scene.add(axes)

  // grid.rotation.x = Math.PI / 2
  axes.rotation.x = Math.PI

  const x = 450
  const y = -200

  camera.position.x = x
  camera.position.y = y
  camera.position.z = 500
  camera.lookAt(x, y, 0)

}

const setTilePosition = (tile: THREE.Mesh, x: number, y: number) => {
  tile.position.x = x * 60 + 30
  if (y % 2) {
    tile.position.x += 30
  }
  tile.position.y = -y * 15 - 15
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
  const url = 'clear01.png'
  const texture = await createTextureFromUri(url)
  const material = createMaterialByTexture(texture)
  for (let y = 0; y < 100; y++){
    for (let x = 0; x < 100; x++){
      const tile = createTile(material)
      scene.add(tile)
      setTilePosition(tile, x, y)
    }
  }
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
  initMouseEvent()
  // init3D(1200, 600)
  await create()
  renderLoop()
}

onMounted(init)
</script>
