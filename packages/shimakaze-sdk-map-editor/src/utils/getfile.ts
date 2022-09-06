export const getImage = async (uri: string) => {
  const image = await fetch(uri)
  const blob = await image.blob()
  return blobToImage(blob)
}

export const blobToImage = (blob: Blob) => new Promise<HTMLImageElement>((resolve, reject) => {
  const fr = new FileReader()
  fr.readAsDataURL(blob)
  fr.onload = e => {
    const img = document.createElement('img')
    img.src = fr.result as string
    img.onload = () => resolve(img)
  }
})
