export const download = (blob: Blob, filename: string) => {
  const aLink = document.createElement('a')
  aLink.href = URL.createObjectURL(blob)
  aLink.setAttribute('download', filename)
  aLink.click()
}
