export default class BackendServiceTools {
  static BuildQuery(queryItems?: any | undefined): string {
    const tempItem = { ...queryItems };

    const keys = Object.keys(tempItem);
    if (keys.length == 0) {
      return "";
    }

    let query = "?";
    keys.forEach((key) => {
      const originalValue = tempItem[key];
      if (BackendServiceTools.SkipQueryParameter(originalValue)) {
        return;
      }

      const value = this.SanitizeQueryParameter(originalValue);
      if (value) {
        query += query === "?" ? "" : "&";
        query += `${key}=${value}`;
      }
    });

    if (query === "?") {
      return "";
    }

    return query;
  }

  static SkipQueryParameter(queryItem: any): boolean {
    const type = typeof queryItem;

    return type === "function" || queryItem === undefined || queryItem === null;
  }

  static SanitizeQueryParameter(queryItem: any): any {
    const type = typeof queryItem;

    if (type === "function") {
      return "";
    }

    if (type === "bigint" || type == "boolean" || type === "number") {
      return queryItem;
    }

    if (type === "string") {
      return encodeURIComponent(queryItem ? queryItem : "");
    }

    const stringified = JSON.stringify(queryItem);
    return encodeURIComponent(stringified);
  }
}
