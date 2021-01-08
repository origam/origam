import { isBase64 } from "./values";

const IMAGE_TYPE: { [key: string]: string } = {
  "/": "jpg",
  i: "png",
  R: "gif",
  U: "webp",
};

export function processedImageURL(value?: string) {
  if (!value) {
    return {
      isEmbedded: false,
      value: null,
    };
  }

  if (isBase64(value)) {
    return { isEmbedded: true, value: `data:image/${IMAGE_TYPE[value.charAt(0)]};base64,${value}` };
  } else {
    return { isEmbedded: false, value };
  }
}
