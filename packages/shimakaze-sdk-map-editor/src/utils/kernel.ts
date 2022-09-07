import * as PIXI from 'pixi.js'

export default class Kernel {
  private static _instance: Kernel
  private _app
  private _container

  constructor (options: PIXI.IApplicationOptions | undefined = undefined) {
    Kernel._instance = this
    this._app = new PIXI.Application(options)
    this._container = new PIXI.Container()

    this._app.stage.addChild(this._container)

    // Move container to the center
    // this._container.x = this._app.screen.width / 2
    // this._container.y = this._app.screen.height / 2

    // Center bunny sprite in local container coordinates
    // this._container.pivot.x = this._container.width / 2
    // this._container.pivot.y = this._container.height / 2

    // Listen for animate update
    this._app.ticker.add((delta) => {
    // rotate the container!
    // use delta to create frame-independent transform
      // this._container.rotation -= 0.01 * delta
    })
  }

  static get instance () {
    return Kernel._instance
  }

  get dom () {
    return this._app.view
  }

  get app () {
    return this._app
  }

  get container () {
    return this._container
  }
}
