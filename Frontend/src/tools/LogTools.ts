import Constants from "../enum/Constants";

export default class LogTools {
  static DebugLog(header: string, message: any) {
    console.log(Constants.DebugSeparator);
    console.log(header);
    console.log(message);
    console.log(Constants.DebugSeparator);
  }
}
