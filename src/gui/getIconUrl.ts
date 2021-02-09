import { IMenuItemIcon } from "./Workbench/MainMenu/MainMenu";

export function getIconUrl(iconName: string | IMenuItemIcon, iconPath?: string) {
  switch (iconName) {
    case IMenuItemIcon.Form:
      return "./icons/document.svg";
    case IMenuItemIcon.Workflow:
      return "./icons/settings.svg";
    case IMenuItemIcon.WorkQueue:
      return "./icons/work-queue.svg";
    case IMenuItemIcon.Chat:
      return "./icons/chat.svg";
    default:
      if (iconPath) {
        return iconPath;
      } else {
        return "./icons/document.svg";
      }
  }
}
