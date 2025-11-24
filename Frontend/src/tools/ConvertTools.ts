export default class ConvertTools {
  static ConvertListToType<T>(
    constructor: new (...args: any[]) => T,
    rawList: any
  ): Array<T> {
    if (rawList && Array.isArray(rawList)) {
      const newList = rawList.map((x) => new constructor(x));
      return newList;
    }

    return [];
  }
}
