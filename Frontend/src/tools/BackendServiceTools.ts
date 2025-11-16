export default class BackendServiceTools {
  static BuildQuery(queryItems?: any | undefined): string {
    const tempItem = { ...queryItems };

    const keys = Object.keys(tempItem);
    if (keys.length == 0) {
      return "";
    }

    let query = "?";
    keys.forEach((key) => {
      const value = this.SanitizeQueryParameter(tempItem[key]);
      if (value) {
        query += query === "?" ? "" : "&";
        query += `${key}=${value}`;
      }
    });

    return query;
  }

  static SanitizeQueryParameter(queryItem: any): any {
    const type = typeof queryItem;

    if (type === "bigint" || type == "boolean" || type === "number") {
      return queryItem;
    }

    if (type === "string") {
      return encodeURIComponent(queryItem ? queryItem : "");
    }

    if (type === "function") {
      return "";
    }

    const stringified = JSON.stringify(queryItem);
    return encodeURIComponent(stringified);
  }
}
