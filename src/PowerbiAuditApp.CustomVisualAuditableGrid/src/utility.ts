export type Style = Partial<
  Omit<
    CSSStyleDeclaration,
    'length' | 'parentRule' | 'getPropertyPriority' | 'getPropertyValue' | 'item' | 'removeProperty' | 'setProperty' | '[index: number]'
  >
>;
export type SortType = 'parseString' | 'parseInteger' | 'parseDecimal' | 'parseDate';

export type PrimitiveType = number | string | boolean | Date;

export const GridConstants = {
  sNAN: 'NaN',
  sNA: ' ', //for null values, this default null string would be displayed
  sParseDate: 'parseDate',
  sParseDollar: 'parseUSD',
  sParseInteger: 'parseInteger',
  sParseFloat: 'parseDecimal',
  sParseString: 'parseString',
  sParseSalesStage: 'parseSalesStage',
  sError: "Unable to copy oObject! Its type isn't supported.",
  sFunction: 'function',
  sPaginationText: 'Jump to',
  sDefaultWidth: '250px',
  iDropDownLimit: 20,
};

// var MAQUtility;
// (function (MAQUtility) {
//   function getParents(oNode, sClassSelector) {
//     var aParents = [],
//       oCurrentNode = oNode.parentNode,
//       oTempNode;
//     while (oCurrentNode !== document) {
//       oTempNode = oCurrentNode;
//       if (!sClassSelector || !sClassSelector.length || -1 < oTempNode.className.indexOf(sClassSelector)) {
//         aParents.push(oTempNode);
//       }
//       oCurrentNode = oTempNode.parentNode;
//     }
//     return aParents;
//   }
//   getParents = getParents;

export function applyStyleToObject(oNode: HTMLElement, oStyleObject: Style) {
  if (typeof oStyleObject === 'undefined') {
    return;
  }
  const oStyles = Object.keys(oStyleObject) as (keyof Style)[];
  for (let iCounter = 0; iCounter < oStyles.length; iCounter += 1) {
    try {
      oNode.style[oStyles[iCounter]] = oStyleObject[oStyles[iCounter]];
    } catch (exception) {}
  }
  return;
}

export function hasClass(oElement: Element, sName: string) {
  if (oElement && oElement.className) {
    return new RegExp('(\\s|^)' + sName + '(\\s|$)').test(oElement.className);
  }
  return;
}

function isNodeList(pet: Element | NodeList): pet is NodeList {
  return (pet as NodeList).length > 0;
}

export function removeClass(element: Element | NodeList, name: string) {
  if (!element) return;
  if (isNodeList(element)) {
    for (let i = 0; i < element.length; i++) {
      removeClass(element[i] as Element, name);
    }
  } else {
    if (hasClass(element, name)) {
      element.className = element.className.replace(new RegExp('(\\s|^)' + name + '(\\s|$)'), ' ').replace(/^\s+|\s+$/g, '');
    }
  }
}

export function addClass(element: Element | NodeList, name: string) {
  if (!element) return;
  if (isNodeList(element)) {
    for (let i = 0; i < element.length; i++) {
      addClass(element[i] as Element, name);
    }
  } else {
    if (!hasClass(element, name)) {
      element.className += (element.className ? ' ' : '') + name;
    }
  }
}

export interface PrimitiveTypeObject {
  [name: string]: PrimitiveType;
}
export function sortBy(field: string, reverse: boolean, primer: SortType) {
  const time = function (x: PrimitiveTypeObject) {
    if (x[field]) {
      return Date.parse(x[field].toString());
    }
    return 0;
  };
  const trimUSD = function (x: PrimitiveTypeObject) {
    if (x[field] && x[field] !== GridConstants.sNA) {
      return parseInt(x[field].toString().substring(1, x[field].toString().length).split(',').join(''));
    }
    return 0;
  };
  const trimSalesStage = function (x: PrimitiveTypeObject) {
    if (x[field] && x[field] !== GridConstants.sNA) {
      var oStageInfo = x[field].toString().split(' ');
      return parseInt(oStageInfo[oStageInfo.length - 1].slice(0, oStageInfo[oStageInfo.length - 1].length - 1));
    }
    return 0;
  };
  const stringConvert = function (x: PrimitiveTypeObject) {
    if (x[field] && x[field] !== GridConstants.sNA) {
      return x[field].toString();
    }
    return 0;
  };
  const parseInteger = function (x: PrimitiveTypeObject) {
    if (x[field]) {
      return parseInt(x[field].toString());
    }
    return 0;
  };
  const parseDecimal = function (x: PrimitiveTypeObject) {
    if (x[field]) {
      return parseFloat(x[field].toString());
    }
    return 0;
  };
  const parseString = function (x: PrimitiveTypeObject) {
    if (x[field]) {
      return x[field].toString();
    }
    return '';
  };
  return function (a: PrimitiveTypeObject, b: PrimitiveTypeObject) {
    var iFirstValue, iSecondValue;
    if (primer === GridConstants.sParseDate) {
      (iFirstValue = time(a)), (iSecondValue = time(b));
    } else if (primer === GridConstants.sParseDollar) {
      (iFirstValue = trimUSD(a)), (iSecondValue = trimUSD(b));
    } else if (primer === GridConstants.sParseSalesStage) {
      (iFirstValue = trimSalesStage(a)), (iSecondValue = trimSalesStage(b));
    } else if (primer === GridConstants.sParseInteger) {
      (iFirstValue = parseInteger(a)), (iSecondValue = parseInteger(b));
    } else if (primer === GridConstants.sParseFloat) {
      (iFirstValue = parseDecimal(a)), (iSecondValue = parseDecimal(b));
    } else if (primer === GridConstants.sParseString) {
      (iFirstValue = parseString(a)), (iSecondValue = parseString(b));
    } else {
      throw 'Unexpected primer type';
    }
    return (iFirstValue < iSecondValue ? -1 : iFirstValue > iSecondValue ? +1 : 0) * [-1, 1][+!!reverse];
  };
}

export function clone<T>(object: T): T {
  // Handle the 3 simple types, and null or undefined
  var copy, attribute;
  if (null === object || 'object' !== typeof object) {
    return object;
  }

  // Handle Date
  if (object instanceof Date) {
    copy = new Date();
    copy.setTime(object.getTime());
    return copy as unknown as T;
  }

  // Handle Array
  if (object instanceof Array) {
    copy = [];
    for (let i = 0; i < object.length; i++) {
      copy[i] = clone(object[i]);
    }
    return copy as unknown as T;
  }

  // Handle Object
  if (object instanceof Object) {
    copy = {} as T;
    for (attribute in object) {
      if (object.hasOwnProperty(attribute)) {
        copy[attribute as keyof typeof object] = clone(object[attribute as keyof typeof object]);
      }
    }
    return copy as T;
  }
  throw new Error(GridConstants.sError);
}

//   // applyFormatter: applies formatting to data
//   function applyFormatter(sText, sFormatterName, oConfiguration, iIterator) {
//     if (typeof oConfiguration === 'undefined') {
//       oConfiguration = {};
//     }
//     if (typeof iIterator === 'undefined') {
//       iIterator = 0;
//     }
//     if (sFormatterName) {
//       if (typeof window[sFormatterName] === 'function') {
//         sText = window[sFormatterName](sText, oConfiguration, iIterator);
//       } else if (typeof sFormatterName === 'function') {
//         sText = sFormatterName(sText, oConfiguration, iIterator);
//       }
//     }
//     return sText;
//   }
//   applyFormatter = applyFormatter;
// })(MAQUtility || (MAQUtility = {}));
// /// <disable>JS2025.InsertSpaceBeforeCommentText</disable>
