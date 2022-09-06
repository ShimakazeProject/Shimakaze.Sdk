export const createImage = (url: string) => new Promise<HTMLImageElement>((resolve, reject) => {
  const image = new Image()
  image.src = url
  image.onload = e => resolve(image)
})
