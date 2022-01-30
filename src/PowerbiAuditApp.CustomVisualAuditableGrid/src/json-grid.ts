import powerbi from 'powerbi-visuals-api';
import * as d3 from 'd3';
import * as utility from './utility';
import { valueFormatter } from 'powerbi-visuals-utils-formattingutils';
import { Style, SortType, PrimitiveType, GridConstants } from './utility';

const $ = (window as any).$ as JQueryStatic;
type SortOrder = 'asc' | 'desc';

export interface ColumnColorStyle {
  columnName: string[];
  color: string[];
}
export interface DataColumn {
  [name: string]: PrimitiveType | ColumnColorStyle;
}

(window as any).gridObjects = (window as any).gridObjects || {};
const gridObjects: { [key: string]: IGridOptions } = (window as any).gridObjects;

(window as any).dataView = (window as any).dataView || {};
const dataViews: { [key: string]: powerbi.DataView } = (window as any).dataView;

interface IGridOptions {
  container?: HTMLElement;

  containerObject?: HTMLElement;
  tblBodyRight?: HTMLTableSectionElement;
  tblFootRight?: HTMLTableSectionElement;
  tblHeadRight?: HTMLTableSectionElement;
  gridObject?: HTMLDivElement | HTMLTableElement;
  tblBody?: HTMLTableSectionElement;
  tblFoot?: HTMLTableSectionElement;
  tblHead?: HTMLTableSectionElement;
  hiddenContainer?: HTMLElement;

  containerName: string;
  gridName: string;
  hiddenName: string;

  data: DataColumn[];
  headerTemplate?: {
    dataID: string;
    style: Style;
    colSpan: number;
    onclick: null;
    headerClassName: string;
    columnText: string;
  }[];
  columnHeader: {
    name: string;
    id?: string;
    columnText: string;
    sortable: boolean;
    sortType: SortType;
    sortKey: string;
    headerClassName: string;
    formatter: string;
    style: Style;
    category: string;
    roles: { [name: string]: boolean };
    chartColor?: string;
    align?: string;
    'data-name'?: string;
    noOverlap?: boolean;
    trimOnOverflow?: boolean;
    chartValueFormatter?: string;
    barColors?: string[];
    sortAttribute?: string;
  }[];
  style?: Style;
  altRowColor?: string;
  gridSort?: {
    sortBy: string;
    sortOrder: SortOrder;
    sortType?: SortType;
  };
  serverGrid?: {
    enabled: boolean;
    totalPages: number;
    currentIndex: number;
    sendRequestFunction: (parameters: {}) => boolean;
  };
  inPlaceGrid?: {
    enableInPlaceGrid: boolean;
    disableHeader: boolean;
    parentContainer: string;
    level: string;
    enableRowInsert: boolean;
  };
  viewRecords?: boolean;
  pagination: {
    maxRows: number;
    retainPageOnSort: boolean;
    paginate: boolean;
    iLast?: number;
  };
  scrolling?: {
    enabled: boolean;
    scrollStyle: {};
  };
  cellSpacing?: number;
  cellPadding?: number;
  rows: {
    alternate: boolean;
    rowClassName?: string;
  };
  endRow?: {
    enableEndRow: boolean;
    isTotalRow: boolean;
    includeFormatters: { [key: string]: string };
    columnsExcluded: string[];
    data: DataColumn;
    endRowPosition: number;
    className: string;
    isSplitRowEnabled: boolean;
    splitRowFormatter: string;
  };
  tooltipData?: { [name: string]: number | string | boolean | Date }[];
  tooltipColumnHeader?: string[];
  clientGrid?: boolean;
  legends?: {
    legendTitle?: string;
    titleStyle?: Style;
    enableLegends: boolean;
    labelFirst: boolean;
    separationStyle: Style;
    containerStyle: Style;
    legendTemplate: {
      label: string;
      labelStyle: Style;
      indicatorStyle: Style;
    }[];
  };
  groupedRows?: boolean;
  groupedRowHeader?: {
    groupHeaderName: [];
    data: {
      name: string;
      columnText: string;
      // sortable: boolean;
      // sortType: SortType;
      // sortKey: string;
      headerClassName: string;
      // formatter: string;
      style: Style;
      // category: string;
      // roles: { [name: string]: boolean };
      // chartColor?: string;
    }[];
  };
  fixedHeaderEnd?: null;
  dataConfiguration?: {
    calculateMaximum: boolean;
    columnsIncluded: [];
    maxConfig: DataColumn;
    includeEndRow: boolean;
    useAbsolutes: [];
    stackedBar: {
      enabled: boolean;
      stackedColumns: [];
      color: [];
      colorMapping: {
        hasMultiColoredBars: boolean;
        mappingColumn: string;
        colorMap: [];
      };
      hasRelativeRows: boolean;
      relateByColumn: string;
      displayRelateByColumn: boolean;
      className: string;
    };
    customSecondaryFormatter: string;
  };
  currentPage?: number;
  totalPages?: number;
  callBackFunc: () => void;
  fetchData: {
    enabled: boolean;
    getNextPage: () => void;
    resetData?: () => void;
    hasMoreData: boolean;
  };
}

export class JsonGridSettings {
  fontSize: number = 12;
  maxRows: number = 2000;
  sortKey: number = 1;
  sortOrder: string = 'asc';
  fetchData: IGridOptions['fetchData'] = {
    enabled: false,
    getNextPage: () => {},
    hasMoreData: false,
  };
}

export function CreateGrid(visualName: string, dataView: powerbi.DataView, gridFormatters: JsonGridSettings) {
  const gridOptions: IGridOptions = {
    containerName: visualName,
    gridName: visualName + '_Grid',
    hiddenName: visualName + '_hidden',
    data: [],
    columnHeader: [],
    viewRecords: false,
    pagination: {
      maxRows: gridFormatters.maxRows,
      retainPageOnSort: false,
      paginate: true,
    },

    fetchData: {
      enabled: gridFormatters.fetchData.enabled,
      hasMoreData: gridFormatters.fetchData.hasMoreData,
      resetData: gridFormatters.fetchData.resetData,
      getNextPage: gridFormatters.fetchData.getNextPage,
    },
    rows: {
      alternate: false,
    },
    tooltipData: [],
    tooltipColumnHeader: [],
    clientGrid: true,
    callBackFunc: null,
  };

  dataViews[gridOptions.gridName] = dataView;
  //This function will convert the date to a string(required format of date)
  function CustomDate(date1: Date) {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const months = [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July',
      'August',
      'September',
      'October',
      'November',
      'December',
    ];
    const dateString = days[date1.getDay()] + ', ' + months[date1.getMonth()] + ' ' + date1.getDate() + ', ' + date1.getFullYear();
    return dateString;
  }

  for (let j = 0; j < dataView.table.rows.length; j++) {
    const rowData: IGridOptions['data'][number] = {};
    const tooltipData: IGridOptions['tooltipData'][number] = {};
    const tooltipColName: IGridOptions['tooltipColumnHeader'] = [];

    for (let i = 0; i < dataView.table.columns.length; i++) {
      const col = dataView.table.columns[i].queryName.replaceAll('"', "'");
      const columnName =
        col.lastIndexOf(')') === col.length - 1
          ? col.substring(col.indexOf('.') + 1, col.lastIndexOf(')')).replaceAll('"', "'")
          : col.substring(col.indexOf('.') + 1, col.length).replaceAll('"', "'");

      const value = dataView.table.rows[j][i];

      if (dataView.table.columns[i].roles['Tooltip']) {
        if (value === null) {
          tooltipData[columnName] = null;
        } else {
          tooltipData[columnName] = value;
        }
        tooltipColName.push(columnName);
      }

      //check if the value is null
      if (value === null) {
        rowData[columnName] = null;
      } else if (typeof value == 'object') {
        if (value && value instanceof Date) {
          rowData[columnName] = CustomDate(value);
        }
      } else if (typeof value == 'number') {
        //check if it's a number
        //Rounding decimal numbers to 2 places, if it is a floating number
        if (value > Math.floor(value)) {
          rowData[columnName] = value;
        } else {
          //when it's not a floating-point number
          rowData[columnName] = value;
        }
      } //if it's other than date and number
      else {
        rowData[columnName] = dataView.table.rows[j][i];
      }
    }
    gridOptions.data.push(rowData);
    gridOptions.tooltipData.push(tooltipData);
    gridOptions.tooltipColumnHeader = tooltipColName;
  }
  gridOptions.columnHeader = [];
  for (let i = 0; i < dataView.table.columns.length; i++) {
    const col = dataView.table.columns[i].queryName.replaceAll('"', "'");

    const columnHeader: IGridOptions['columnHeader'][number] = {
      name:
        col.lastIndexOf(')') === col.length - 1
          ? col.substring(col.indexOf('.') + 1, col.lastIndexOf(')')).replaceAll('"', "'")
          : col.substring(col.indexOf('.') + 1, col.length),
      columnText: dataView.table.columns[i].displayName.replaceAll('"', "'"),
      sortable: !gridOptions.fetchData.enabled,
      sortType: 'parseString',
      sortKey:
        col.lastIndexOf(')') === col.length - 1
          ? col.substring(col.indexOf('.') + 1, col.lastIndexOf(')')).replaceAll('"', "'")
          : col.substring(col.indexOf('.') + 1, col.length).replaceAll('"', "'"),

      headerClassName: 'TableHeader',
      formatter: '',
      style: {
        width: '350px',
      },
      category: 'unknown', //dataView.table.columns[iCount].type.category,
      roles: dataView.table.columns[i].roles,
    };

    //find first non-null value for the column to know its data type to select right sortType
    let loopCtr = 0;
    while (loopCtr < dataView.table.rows.length && !dataView.table.rows[loopCtr][i]) {
      loopCtr++;
    }
    const sort_type = loopCtr == dataView.table.rows.length ? typeof dataView.table.rows[0][i] : typeof dataView.table.rows[loopCtr][i];

    switch (sort_type) {
      case 'number':
        columnHeader.sortType = 'parseInteger';
        break;
      case 'object':
        if (loopCtr < dataView.table.rows.length && dataView.table.rows[loopCtr][i] instanceof Date) {
          columnHeader.sortType = 'parseDate';
        }
        break;
    }
    gridOptions.columnHeader.push(columnHeader);
  }

  const col = dataView.table.columns[0].queryName;
  const gridSort: IGridOptions['gridSort'] = {
    sortBy: col.substring(col.indexOf('.') + 1, col.length).replaceAll('"', "'"),
    sortOrder: 'asc',
    sortType: 'parseString',
  };
  let sortKey = gridFormatters.sortKey;
  let sortOrder = gridFormatters.sortOrder;
  let tooltipColumns = 0;
  for (let i = 0; i < dataView.table.columns.length; i++) if (dataView.table.columns[i].roles['Tooltip']) tooltipColumns++;
  let sKey = 0;

  if (sortKey > dataView.table.columns.length - tooltipColumns) {
    //invalid sortKey with value more than no. of columns
    for (let i = dataView.table.columns.length - 1; i >= 0; i--)
      if (dataView.table.columns[i].roles['Values']) {
        sortKey = i + 1;

        gridFormatters.sortKey = dataView.table.columns.length - tooltipColumns;
        break;
      }
  } else if (sortKey < 0) {
    //invalid sortKey with value less than 0
    for (let i = 0; i < dataView.table.columns.length; i++)
      if (dataView.table.columns[i].roles['Values']) {
        sortKey = i + 1;
        gridFormatters.sortKey = 1;
        break;
      }
  } else {
    for (let i = 0; i < dataView.table.columns.length; i++)
      if (dataView.table.columns[i].roles['Values']) {
        sKey++;
        if (sKey == sortKey) {
          sortKey = i + 1;
          break;
        }
      }
  }
  sortKey--;
  const queryName = dataView.table.columns[sortKey].queryName;
  gridSort.sortBy = queryName.substring(queryName.indexOf('.') + 1, queryName.length).replaceAll('"', "'");
  gridSort.sortOrder = sortOrder && 'desc' == sortOrder.toLowerCase() ? 'desc' : 'asc';

  switch (typeof dataView.table.rows[0][sortKey]) {
    case 'number':
      gridSort.sortType = 'parseDecimal'; //to sort numeric data, be it integers or floating-point numbers.
      break;
    case 'object':
      if (dataView.table.rows[0][sortKey] instanceof Date) {
        gridSort.sortType = 'parseDate';
      }
      break;
  }
  gridOptions.gridSort = gridSort;
  $('#' + gridOptions.containerName).text('');

  JsonGrid(gridOptions);
  d3.select('.DataDiv').style('font-size', gridFormatters.fontSize + 'px');
  d3.select('.first').style('width', gridFormatters.fontSize + 'px');
  d3.select('.next').style('width', gridFormatters.fontSize + 'px');
}

function insertCommasOnly(input: number, decimalPlaces: number) {
  if (!input || isNaN(input)) {
    return 'N/A';
  }

  // Check for validity of decimal places parameter
  if (!decimalPlaces || isNaN(decimalPlaces)) {
    decimalPlaces = 0; // Default value is 0
  }
  let sTempValue = input.toString();

  if (-1 !== sTempValue.indexOf('.')) {
    var decimalLength = sTempValue.substring(sTempValue.indexOf('.') + 1).length;
    if (decimalPlaces < decimalLength) {
      sTempValue = input.toFixed(decimalPlaces).toString();
    }
  }
  var aDigits = sTempValue.split('.'),
    sIntegerDigits = aDigits[0],
    sFractionDigits = aDigits.length > 1 ? '.' + aDigits[1] : '';

  sIntegerDigits = sIntegerDigits.toString();
  var rPattern = /(\d+)(\d{3})/;
  while (rPattern.test(sIntegerDigits)) {
    sIntegerDigits = sIntegerDigits.replace(rPattern, '$1' + ',' + '$2');
  }

  var finalValue = sIntegerDigits + sFractionDigits;
  if (0 === parseFloat(finalValue)) {
    return '0';
  }
  return finalValue;
}

function newRecords(element: HTMLElement, gridName: string) {
  if (element) {
    let pageId = $(element).attr('data-pageId');
    if (!pageId) {
      element = element.childNodes[0] as HTMLElement;
      pageId = $(element).attr('data-pageId');
    }
    if (pageId) {
      (window as any).pageId = pageId;
      if (document.getElementsByClassName('ListOptionContainer') && document.getElementsByClassName('ListOptionContainer')[0])
        (document.getElementsByClassName('ListOptionContainer')[0] as HTMLInputElement).value = pageId;

      let currentPage = parseInt(element.getAttribute('data-pageId'));
      let lastPage = gridObjects[gridName].totalPages + 1;
      utility.addClass(element, 'SelectedPage');
      if (currentPage) {
        getPage(currentPage, lastPage, gridObjects[gridName]);
      }
    }
  }
}

function getAdjustedRowChunk(inputData: DataColumn[string], width: string) {
  return "<div class='jsonGridOverflow' title='" + inputData + "' style='width: " + width + "px;'>' + inputData + '</div>";
}

function setViewRecords(gridOptions: IGridOptions) {
  const gridElement = document.getElementById(gridOptions.gridName);
  const currentPage = (gridOptions.currentPage || 0) + 1;
  const lastPage = (gridOptions.totalPages || 0) + 1;

  utility.removeClass(gridElement.querySelectorAll('.PageListItem'), 'SelectedPage');
  utility.removeClass(gridElement.querySelectorAll('.ListOption'), 'SelectedPage');
  utility.addClass(gridElement.querySelector(".PageListItem[data-pageId='" + currentPage + "']"), 'SelectedPage');

  const elementDropDown = gridElement.querySelector<HTMLOptionElement>(".ListOption[data-pageId='" + currentPage + "']");
  if (elementDropDown) {
    utility.addClass(elementDropDown, 'SelectedPage');
    elementDropDown.selected = true;
  }

  // Create Pagination list
  const totalPages = lastPage < currentPage + 4 ? lastPage : currentPage + 4;
  generatePageList(gridOptions, currentPage, totalPages);
  if (gridOptions.serverGrid.enabled) {
    const hiddenContainer = document.getElementById(gridOptions.hiddenName);
    if (hiddenContainer) {
      hiddenContainer.setAttribute('data-currentPage', gridOptions.currentPage.toString());
    }
    callService(gridOptions);
  } else {
    populateGrid(gridOptions);
  }
}

// Generate page list to be displayed in pagination control
function generatePageList(gridOptions: IGridOptions, currentPage: number, totalPages: number) {
  var gridElement = gridOptions.container || document.getElementById(gridOptions.containerName);

  const pageList = gridElement.querySelector('.ViewRecordDiv > div');

  // Change page numbers
  if (pageList) {
    // Clear existing page list
    $(pageList).text('');
    let startIndex = totalPages - 4 > 0 && currentPage >= totalPages - 4 ? totalPages - 4 : currentPage;
    startIndex = totalPages <= 5 ? 1 : startIndex;
    for (let i = startIndex; i <= totalPages; i++) {
      // Regenerate pages
      const page = document.createElement('div');
      page.innerText = insertCommasOnly(i, 0);

      if (i === currentPage) {
        utility.addClass(page, 'SelectedPage');
      }
      utility.addClass(page, 'PageListItem');
      page.setAttribute('data-pageId', i.toString());
      page.addEventListener('click', function () {
        newRecords(this, gridOptions.gridName);
      });
      pageList.appendChild(page);
    }
  }
}
function getPage(currentPageNum: number, lastPageNum: number, gridOptions: IGridOptions) {
  const currentPage = currentPageNum;
  const lastPage = lastPageNum;
  const first = document.getElementById(gridOptions.gridName + '_First');
  const last = document.getElementById(gridOptions.gridName + '_Last');

  if (currentPage <= 1) {
    goFirst(first, gridOptions.gridName);
  } else if (currentPage >= lastPage) {
    goLast(last, gridOptions.gridName);
  } else {
    // Go to respective page
    gridOptions.currentPage = currentPageNum - 1;
    if (!gridOptions.serverGrid.enabled) {
      enablePrev(gridOptions.gridName);
      enableNext(gridOptions.gridName);
    }
    setViewRecords(gridOptions);
  }
}
function populateGrid(gridOptions: IGridOptions) {
  const htmlGridObject = gridOptions.gridObject;
  if (htmlGridObject) {
    const numberOfRows = gridOptions.tblBody.rows.length;
    for (let i = 0; i < numberOfRows; i += 1) {
      gridOptions.tblBody.deleteRow(-1);
    }
    if (gridOptions.fixedHeaderEnd) {
      for (let i = 0; i < numberOfRows; i += 1) {
        gridOptions.tblBodyRight.deleteRow(-1);
      }
    }
    CreateHTMLTableRow(gridOptions);
    gridOptions.callBackFunc && gridOptions.callBackFunc();
  }
}

function disablePrev(gridName: string) {
  var previous = document.getElementById(gridName + '_Prev');
  utility.addClass(previous, 'click-disabled');
}
function enablePrev(gridName: string) {
  var previous = document.getElementById(gridName + '_Prev');
  utility.removeClass(previous, 'click-disabled');
}
function disableNext(gridName: string) {
  var next = document.getElementById(gridName + '_Next');
  utility.addClass(next, 'click-disabled');
}
function enableNext(gridName: string) {
  var next = document.getElementById(gridName + '_Next');
  utility.removeClass(next, 'click-disabled');
}
function goLast(element: HTMLElement, gridName: string) {
  if (!utility.hasClass(element, 'click-disabled')) {
    if (gridName.length) {
      if (gridObjects[gridName]) {
        const currentGridConfiguration = gridObjects[gridName];
        currentGridConfiguration.currentPage = currentGridConfiguration.totalPages;
        setViewRecords(currentGridConfiguration);
        if (!currentGridConfiguration.serverGrid.enabled) {
          disableNext(currentGridConfiguration.gridName);
          enablePrev(currentGridConfiguration.gridName);
        }
      }
    }
  }
}
function goFirst(element: HTMLElement, gridName: string) {
  if (!utility.hasClass(element, 'click-disabled')) {
    if (gridName.length) {
      if (gridObjects[gridName]) {
        const gridOptions = gridObjects[gridName];
        gridOptions.currentPage = 0;
        setViewRecords(gridOptions);
        if (!gridOptions.serverGrid.enabled) {
          disablePrev(gridOptions.gridName);
          enableNext(gridOptions.gridName);
          populateGrid(gridOptions);
        }
      }
    }
  }
}
function goPrevious(element: HTMLElement, gridName: string) {
  if (!utility.hasClass(element, 'click-disabled')) {
    if (gridName.length > 0) {
      if (gridObjects[gridName]) {
        const gridOptions = gridObjects[gridName];
        const pageId = gridOptions.currentPage;
        if (document.getElementsByClassName('ListOptionContainer') && document.getElementsByClassName('ListOptionContainer')[0])
          (document.getElementsByClassName('ListOptionContainer')[0] as HTMLInputElement).value = pageId.toString();
        gridOptions.currentPage = parseInt(gridOptions.currentPage.toString()) - 1;
        setViewRecords(gridOptions);
        if (!gridOptions.serverGrid.enabled) {
          if (!gridOptions.currentPage) {
            disablePrev(gridOptions.gridName);
            enableNext(gridOptions.gridName);
          }
          if (parseInt(gridOptions.currentPage.toString()) > 0) {
            enableNext(gridOptions.gridName);
          }
        }
      }
    }
  }
}
function goNext(element: HTMLElement, gridName: string) {
  var gridObjectPosition, gridOptions;
  if (!utility.hasClass(element, 'click-disabled')) {
    if (gridName.length > 0) {
      gridObjectPosition = gridName.indexOf(gridName);
      if (gridObjects[gridName]) {
        gridOptions = gridObjects[gridName];
        gridOptions.currentPage = parseInt(gridOptions.currentPage.toString()) + 1;
        const pageId = gridOptions.currentPage + 1;
        if (document.getElementsByClassName('ListOptionContainer') && document.getElementsByClassName('ListOptionContainer')[0])
          (document.getElementsByClassName('ListOptionContainer')[0] as HTMLInputElement).value = pageId.toString();
        setViewRecords(gridOptions);
        if (!gridOptions.serverGrid.enabled) {
          if (gridOptions.currentPage > 0) {
            enablePrev(gridOptions.gridName);
          }
          if (gridOptions.currentPage === gridOptions.totalPages) {
            disableNext(gridOptions.gridName);
            enablePrev(gridOptions.gridName);
          }
        }
      }
    }
  }
}

// Check if text input is a number and call method to navigate to requested page
function isNumber(event: KeyboardEvent, element: HTMLInputElement, gridName: string) {
  let charCode;
  event = event ? event : (window.event as KeyboardEvent);
  charCode = event.which ? event.which : event.keyCode;
  if (charCode > 31 && charCode !== 13 && (charCode < 48 || charCode > 57)) {
    return false;
  } else {
    goToPage(charCode, element, gridName);
  }
  return true;
}

// Navigate to page entered in text box
function goToPage(charCode: number, element: HTMLInputElement, gridName: string) {
  if (element && element.value) {
    const currentPage = parseInt(element.value);
    if (charCode === 13 && currentPage) {
      if (gridObjects[gridName]) {
        const currentGridConfig = gridObjects[gridName];
        const lastPage = currentGridConfig.totalPages + 1;
        getPage(currentPage, lastPage, currentGridConfig);
      }
    }
  }
}
function sortDataWithinGroup(gridConfiguration: IGridOptions, fieldName: string, sortFlag: boolean, sortType: SortType) {
  let iTotal = gridConfiguration.groupedRowHeader.groupHeaderName.length,
    iInnerCount = 0,
    iInnerTotal = gridConfiguration.data.length,
    arrTemp: IGridOptions['data'] = [],
    arrSortedMerged: IGridOptions['data'] = [];

  for (let iCount = 0; iCount < iTotal; iCount++) {
    arrTemp = [];
    for (iInnerCount = 0; iInnerCount < iInnerTotal; iInnerCount++) {
      if (gridConfiguration.groupedRowHeader.groupHeaderName[iCount] === gridConfiguration.data[iInnerCount].groupHeaderName) {
        arrTemp.push(utility.clone(gridConfiguration.data[iInnerCount]));
      }
    }
    arrSortedMerged = arrSortedMerged.concat(utility.clone(arrTemp.sort(utility.sortBy(fieldName, sortFlag, sortType))));
  }
  return utility.clone(arrSortedMerged);
}

function sortJsonGrid(cellObject: HTMLTableCellElement, gridName: string, fieldName: string) {
  if (gridName.length > 0) {
    (window as any).pageId = 1;
    const listOptionsContainer = document.getElementsByClassName('ListOptionContainer') as HTMLCollectionOf<HTMLSelectElement>;
    if (listOptionsContainer && listOptionsContainer[0]) listOptionsContainer[0].value = '1';

    if (gridObjects[gridName]) {
      const sortOrder = cellObject.getAttribute('sortOrder');
      const sortKey = cellObject.getAttribute('sortKey');
      let sortFlag = false;
      if ('asc' === sortOrder) {
        sortFlag = true;
        cellObject.setAttribute('sortOrder', 'desc');
      } else {
        cellObject.setAttribute('sortOrder', 'asc');
      }
      const gridOptions = gridObjects[gridName];
      gridOptions.gridSort.sortBy = fieldName;

      if (gridOptions.serverGrid.enabled) {
        // Call service in case of service side grid
        var oHiddenContainer = document.getElementById(gridOptions.container + '_hidden');
        if (oHiddenContainer) {
          // Update sort order and sort by data values in hidden chunk
          oHiddenContainer.setAttribute('data-sortOrder', sortOrder);
          oHiddenContainer.setAttribute('data-sortKey', sortKey);
          oHiddenContainer.setAttribute('data-sortBy', fieldName);
          oHiddenContainer.setAttribute('data-currentPage', '0');
        }
        gridOptions.currentPage = 0;
        callService(gridOptions);
      } else {
        let sortType: SortType;
        for (let columnCounter in gridOptions.columnHeader) {
          if (gridOptions.columnHeader[columnCounter].name === fieldName && gridOptions.columnHeader[columnCounter].sortType) {
            if (gridOptions.columnHeader[columnCounter].sortAttribute) {
              fieldName = gridOptions.columnHeader[columnCounter].sortAttribute;
            }
            sortType = gridOptions.columnHeader[columnCounter].sortType;
          }
        }
        if (gridOptions.groupedRows && gridOptions.groupedRowHeader && gridOptions.groupedRowHeader.groupHeaderName) {
          gridOptions.data = sortDataWithinGroup(gridOptions, fieldName, sortFlag, sortType);
        } else {
          gridOptions.data.sort(utility.sortBy(fieldName, sortFlag, sortType));
        }
        gridOptions.gridSort.sortBy = fieldName;
        const sortIndicators = document.querySelectorAll('#' + gridName + ' .SortIndicator');
        for (let i = 0; i < sortIndicators.length; i++) {
          utility.addClass(sortIndicators[i], 'itemHide');
        }

        const sortIndicatorsRight = document.querySelectorAll('#' + gridName + '_right .SortIndicator');
        for (let i = 0; i < sortIndicatorsRight.length; i++) {
          utility.addClass(sortIndicatorsRight[i], 'itemHide');
        }

        utility.removeClass(document.querySelector('#' + gridName + 'Head .jsonGridHeaderAlternate'), 'jsonGridHeaderAlternate');
        const arrow = cellObject.querySelector('.sort' + fieldName.replace(/[^a-zA-z0-9]/g, '_') + 'Hand');
        const span = document.createElement('span');
        $('.asc').remove();
        $('.desc').remove();
        if ('asc' === cellObject.getAttribute('sortOrder')) {
          utility.addClass(span, 'desc');
        } else {
          utility.addClass(span, 'asc');
        }
        arrow.appendChild(span);
        utility.removeClass(arrow, 'itemHide');
        if (arrow) {
          utility.addClass(arrow.parentNode as Element, 'jsonGridHeaderAlternate');
        }
        if (!gridOptions.pagination.retainPageOnSort && gridOptions.totalPages) {
          gridOptions.currentPage = 0;
          setViewRecords(gridOptions);
          disablePrev(gridOptions.gridName);
          enableNext(gridOptions.gridName);
        } else {
          populateGrid(gridOptions);
        }
      }
    }
  }
}

function CreatePaginationControl(gridOptions: IGridOptions) {
  const paginationSpaceRow = gridOptions.tblFoot.insertRow(0);
  const row = gridOptions.tblFoot.insertRow(1);
  const listOptionContainerParent = document.createElement('div');
  const currentPage = gridOptions.currentPage + 1;
  const lastPage = gridOptions.totalPages + 1;
  const listOptionContainer = lastPage > GridConstants.iDropDownLimit ? document.createElement('input') : document.createElement('select');

  if (gridOptions.fixedHeaderEnd) {
    if (gridOptions.containerObject.clientWidth - gridOptions.tblBody.clientWidth >= gridOptions.tblBodyRight.clientWidth) {
      const rightRow = gridOptions.tblFootRight.insertRow(-1);

      const jsonPaginationMargin = rightRow.insertCell(0);
      jsonPaginationMargin.colSpan = gridOptions.columnHeader.length;
      utility.addClass(jsonPaginationMargin, 'jsonPaginationMargin');

      const jsonFooter = rightRow.insertCell(0);
      jsonFooter.colSpan = gridOptions.columnHeader.length;
      utility.addClass(jsonFooter, 'jsonFooter');
    } else {
      const leftGrid = document.querySelector('#' + gridOptions.containerName + ' .LeftGrid') as HTMLDivElement;
      const rightGrid = document.querySelector('#' + gridOptions.containerName + ' .RightGrid') as HTMLDivElement;
      rightGrid.style.height = rightGrid.clientHeight + (leftGrid.clientHeight - rightGrid.clientHeight + 29) + 'px';
      utility.addClass(rightGrid, 'paginationBorder');
    }
  }

  // Update properties of pagination space
  const spaceCell = paginationSpaceRow.insertCell(0);
  spaceCell.colSpan = gridOptions.columnHeader.length;
  utility.addClass(spaceCell, 'jsonPaginationMargin');

  // Cell containing the pagination content
  const cell = row.insertCell(0);
  cell.colSpan = gridOptions.columnHeader.length;
  utility.addClass(cell, 'jsonFooter');

  const label = document.createElement('div');
  label.innerText = GridConstants.sPaginationText;
  utility.addClass(label, 'jsonFooterLabel');

  const prevSpan = document.createElement('span');
  utility.addClass(prevSpan, 'prev');
  utility.addClass(prevSpan, 'cur-pointer');
  $(prevSpan).attr('id', gridOptions.gridName + '_Prev');
  $(prevSpan).attr('active', '1');
  prevSpan.onclick = () => goPrevious(prevSpan, gridOptions.gridName);
  prevSpan.innerText = '<';
  if (0 === gridOptions.currentPage) {
    utility.addClass(prevSpan, 'click-disabled');
  }

  const nextDiv = document.createElement('div');
  nextDiv.className = 'PaginationNextArrowDiv';
  if (lastPage - 1 !== gridOptions.currentPage) {
    const nextSpan = document.createElement('span');
    utility.addClass(nextSpan, 'next');
    utility.addClass(nextSpan, 'cur-pointer');
    $(nextSpan).attr('id', gridOptions.gridName + '_Next');
    $(nextSpan).attr('active', '0');
    nextSpan.onclick = () => goNext(nextSpan, gridOptions.gridName);
    nextSpan.innerText = '>';
    nextDiv.appendChild(nextSpan);
  }

  const dropDownContainer = document.createElement('div');
  dropDownContainer.id = gridOptions.gridName + '_DropDownRecords';
  utility.addClass(dropDownContainer, 'DropDownRecords');

  const totalPagesLabel = document.createElement('div');
  totalPagesLabel.innerText = ' of ' + insertCommasOnly(lastPage, 0);
  utility.addClass(totalPagesLabel, 'jsonFooterLabel totalPagesLabel');

  const paginationContainer = document.createElement('div');
  paginationContainer.className = 'jsonGridFooter';

  const previousDiv = document.createElement('div');
  previousDiv.className = 'PaginationPrevArrowDiv';
  previousDiv.appendChild(prevSpan);
  paginationContainer.appendChild(previousDiv);

  const viewRecords = document.createElement('div');
  viewRecords.id = gridOptions.gridName + '_ViewRecords';
  utility.addClass(viewRecords, 'ViewRecordDiv');
  if (!gridOptions.viewRecords) {
    viewRecords.style.visibility = 'hidden';
  }

  paginationContainer.appendChild(viewRecords);

  paginationContainer.appendChild(nextDiv);
  paginationContainer.appendChild(label);
  paginationContainer.appendChild(dropDownContainer);
  paginationContainer.appendChild(totalPagesLabel);

  cell.appendChild(paginationContainer);

  // Create Page List
  if (viewRecords) {
    const pageList = document.createElement('div');
    viewRecords.appendChild(pageList);
    const totalPages = lastPage < currentPage + 4 ? lastPage : currentPage + 4;
    generatePageList(gridOptions, currentPage, totalPages);
  }

  // Create drop down
  utility.addClass(listOptionContainerParent, 'ListOptionContainerParent');
  utility.addClass(listOptionContainer, 'ListOptionContainer');

  listOptionContainerParent.appendChild(listOptionContainer);
  dropDownContainer.appendChild(listOptionContainerParent);
  if (lastPage > GridConstants.iDropDownLimit) {
    // Create text box
    (listOptionContainer as HTMLInputElement).type = 'text';
    listOptionContainer.value = (gridOptions.currentPage + 1).toString();
    listOptionContainer.addEventListener('keypress', function (e: KeyboardEvent) {
      isNumber(e, this, gridOptions.containerName);
    });
  } else {
    for (let i = 1; i <= lastPage; i++) {
      const listOption = document.createElement('option');
      listOption.innerText = i.toString();
      utility.addClass(listOption, 'ListOption');
      listOption.setAttribute('data-pageId', i.toString());
      listOption.addEventListener('click', function (e) {
        newRecords(this, gridOptions.gridName);
      });
      listOptionContainer.appendChild(listOption);
    }
    const oSelectedElement = document.querySelector(
      '#' + gridOptions.gridName + " .ListOption[data-pageId='" + currentPage + "']"
    ) as HTMLOptionElement;
    utility.addClass(oSelectedElement, 'SelectedPage');
    oSelectedElement.selected = true;
  }

  if (gridOptions.pagination.maxRows && gridOptions.pagination.maxRows > gridOptions.data.length) {
    if (!gridOptions.serverGrid.enabled) {
      utility.addClass(document.querySelector('#' + gridOptions.gridName + '_Last'), 'click-disabled');
      disableNext(gridOptions.gridName);
    }
  }
}

function CreateFetchControl(gridOptions: IGridOptions) {
  const currentPage = (window as any).fetchCurrentPage || 1;
  const fetchSpaceRow = gridOptions.tblFoot.insertRow(0);
  const row = gridOptions.tblFoot.insertRow(1);

  // Update properties of pagination space
  const spaceCell = fetchSpaceRow.insertCell(0);
  spaceCell.colSpan = gridOptions.columnHeader.length;
  spaceCell.className = 'jsonPaginationMargin';

  // Cell containing the pagination content
  const cell = row.insertCell(0);
  cell.className = 'jsonFooter';
  cell.colSpan = gridOptions.columnHeader.length;

  const paginationContainer = document.createElement('div');
  paginationContainer.className = 'jsonGridFooter fetch-bar';

  cell.appendChild(paginationContainer);

  if (gridOptions.fetchData.resetData) {
    const resetDataBtn = document.createElement('button');
    resetDataBtn.className = 'fetch-button cur-pointer';
    $(resetDataBtn).attr('active', '0');
    resetDataBtn.onclick = () => {
      (window as any).fetchCurrentPage = 1;
      gridOptions.fetchData.resetData();
    };
    resetDataBtn.innerText = 'Return to first page';
    paginationContainer.appendChild(resetDataBtn);
    if (currentPage <= 1) {
      resetDataBtn.style.visibility = 'hidden';
    }
  }
  const pagesLabel = document.createElement('span');
  pagesLabel.innerText = `Page: ${currentPage}`;
  pagesLabel.className = 'jsonFooterLabel fetch-label';
  paginationContainer.appendChild(pagesLabel);

  const fetchDataBtn = document.createElement('button');
  fetchDataBtn.className = 'fetch-button cur-pointer';
  $(fetchDataBtn).attr('active', '0');

  fetchDataBtn.onclick = () => {
    (window as any).fetchCurrentPage = currentPage + 1;
    gridOptions.fetchData.getNextPage();
  };
  fetchDataBtn.innerText = 'Load Next Page';
  paginationContainer.appendChild(fetchDataBtn);

  if (!gridOptions.fetchData.hasMoreData) {
    fetchDataBtn.style.visibility = 'hidden';
  }
}

function CreateHTMLTableRow(gridOptions: IGridOptions) {
  var originalGridData = JSON.parse(JSON.stringify(gridOptions.data));

  for (let j = 0; j < gridOptions.data.length; j++) {
    for (let i = 0; i < dataViews[gridOptions.gridName].table.columns.length; i++) {
      const col = dataViews[gridOptions.gridName].table.columns[i].queryName.replaceAll('"', "'");

      const name =
        col.lastIndexOf(')') === col.length - 1
          ? col.substring(col.indexOf('.') + 1, col.lastIndexOf(')')).replaceAll('"', "'")
          : col.substring(col.indexOf('.') + 1, col.length).replaceAll('"', "'");

      let format = dataViews[gridOptions.gridName].table.columns[i].format;
      const formatter = valueFormatter.create({ format: format });
      if (format) {
        gridOptions.data[j][name] = formatter.format(gridOptions.data[j][name]);
      }
    }
  }
  const numberOfColumns = gridOptions.columnHeader.length;
  const isGroupedRow = gridOptions.groupedRows && gridOptions.groupedRowHeader && gridOptions.groupedRowHeader.data;
  const startIndex = gridOptions.serverGrid.enabled ? 0 : gridOptions.currentPage * gridOptions.pagination.maxRows;

  let endIndex =
    startIndex + gridOptions.pagination.maxRows <= gridOptions.data.length
      ? startIndex + gridOptions.pagination.maxRows
      : gridOptions.data.length;
  endIndex = endIndex <= 0 ? gridOptions.data.length : endIndex;

  if (gridOptions.dataConfiguration.calculateMaximum) {
    calculateMinMax(gridOptions, startIndex, endIndex);
  }

  let iTotalGroupedRows = 0;
  if (isGroupedRow) {
    iTotalGroupedRows = gridOptions.groupedRowHeader.data.length;
  }

  let insert: Element;
  let insertRowAt: number;
  let appendAfterRowId: string;
  let parentRowId: string;
  if ('undefined' !== gridOptions.inPlaceGrid.parentContainer && gridOptions.inPlaceGrid.parentContainer) {
    var parentBodyContainer = document.getElementById(gridOptions.inPlaceGrid.parentContainer).getElementsByTagName('tbody')[0];
    gridOptions.tblBody = parentBodyContainer;

    appendAfterRowId = gridOptions.gridName.substr(0, gridOptions.gridName.lastIndexOf('_')) || '';
    insert = document.querySelector('#' + gridOptions.inPlaceGrid.parentContainer + ' #' + appendAfterRowId);
    parentRowId = insert.id;
    insertRowAt = getChildPosition(insert, parentBodyContainer);
  }

  let groupedRowIndex = 0;
  let cellCounter = 0;
  for (let rowPosition = startIndex; rowPosition < endIndex; rowPosition += 1) {
    let iCellCounter = 0;
    let iCellCounterRight = 0;
    const counter = rowPosition as number;

    let row: HTMLTableRowElement;
    if ('undefined' !== gridOptions.inPlaceGrid.parentContainer && gridOptions.inPlaceGrid.parentContainer) {
      row = (insert.parentNode as HTMLTableElement).insertRow(insertRowAt + rowPosition + 1);
    } else {
      row = gridOptions.tblBody.insertRow(-1);
    }

    let oRowRight: HTMLTableRowElement;
    if (gridOptions.fixedHeaderEnd) {
      const oRowRight = gridOptions.tblBodyRight.insertRow(-1);
      utility.addClass(oRowRight, 'GridRow');
    }
    if (isGroupedRow) {
      // Insert group header before first data row
      if (counter === startIndex || gridOptions.data[counter - 1]['groupHeaderName'] !== gridOptions.data[counter]['groupHeaderName']) {
        let iCurrentRowIndex;
        if (counter !== startIndex) {
          groupedRowIndex = 0;
          iCurrentRowIndex = 0;
        } else {
          iCurrentRowIndex = groupedRowIndex + counter;
        }
        const sHeaderName = gridOptions.data[counter]['groupHeaderName'];
        let iHeaderIndex = 0;
        gridOptions.groupedRowHeader.data.forEach(function (element) {
          if (element.name === sHeaderName) {
            iHeaderIndex = gridOptions.groupedRowHeader.data.indexOf(element);
          }
        });
        const groupHeaderRow = gridOptions.tblBody.insertRow(iCurrentRowIndex);
        const td = document.createElement('td');
        //throw 'utility.colSpan(td, numberOfColumns)';
        utility.addClass(td, gridOptions.groupedRowHeader.data[iHeaderIndex].headerClassName);

        utility.addClass(groupHeaderRow, 'GroupHeaderRow');
        if (gridOptions.groupedRowHeader.data[iHeaderIndex].style) {
          utility.applyStyleToObject(groupHeaderRow, gridOptions.groupedRowHeader.data[iHeaderIndex].style);
        }
        groupedRowIndex++;
      }
    }
    if (!gridOptions.rows.alternate) {
      utility.addClass(row, gridOptions.rows.rowClassName || 'GridRow');
    } else if (rowPosition % 2) {
      utility.addClass(row, (gridOptions.rows.rowClassName || 'GridRow') + '_alt');
    } else {
      utility.addClass(row, gridOptions.rows.rowClassName || 'GridRow');
    }
    if ('undefined' !== typeof gridOptions.inPlaceGrid && gridOptions.inPlaceGrid.level) {
      var level = gridOptions.inPlaceGrid.level;
      if ('' === gridOptions.inPlaceGrid.parentContainer) {
        appendAfterRowId = gridOptions.containerName + '_' + level + '_Row' + (rowPosition + 1) + '_';
        row.setAttribute('id', appendAfterRowId);
      } else {
        var sAllParentContainer = level + '_Row' + (rowPosition + 1) + parentRowId;
        row.setAttribute('data-rowParentID', appendAfterRowId);
        row.setAttribute('id', sAllParentContainer);
      }

      utility.addClass(row, level || 'GridRow');
    }

    for (cellCounter = 0; cellCounter < numberOfColumns; cellCounter++) {
      if (gridOptions.columnHeader[cellCounter].roles['Values']) {
        let cell: HTMLTableCellElement;
        if (gridOptions.fixedHeaderEnd && parseInt(gridOptions.fixedHeaderEnd) <= cellCounter) {
          cell = oRowRight.insertCell(iCellCounterRight++);
        } else {
          cell = row.insertCell(iCellCounter++);
        }
        cell.setAttribute('class', 'jsonGridRow');
        cell.style.textAlign = gridOptions.columnHeader[cellCounter].align;

        // To add which report to invoke
        if (gridOptions.columnHeader[cellCounter]['data-name']) {
          cell.setAttribute('data-name', gridOptions.columnHeader[cellCounter]['data-name']);
        }

        // Doing this to avoid overlapping of last column values and the scroll bar
        if (gridOptions.columnHeader[cellCounter].noOverlap) {
          if (cellCounter === numberOfColumns - 1) {
            cell.style.paddingRight = String(20) + 'px';
            utility.addClass(cell, 'noOverlap');
          }
        }

        //// TODO: Update code below to add style.
        if (gridOptions.altRowColor && rowPosition % 2 !== 0) {
          cell.style.backgroundColor = gridOptions.altRowColor;
        }
        if (gridOptions.columnHeader[cellCounter].style) {
          utility.applyStyleToObject(cell, gridOptions.columnHeader[cellCounter].style);
        }

        let returnValue: DataColumn[string];
        if (!gridOptions.columnHeader[cellCounter].formatter) {
          if (gridOptions.columnHeader[cellCounter].trimOnOverflow) {
            returnValue = getAdjustedRowChunk(
              gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name] || GridConstants.sNA,
              gridOptions.columnHeader[cellCounter].style.width
            );
          } else {
            returnValue = gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name] || GridConstants.sNA;
          }
        } else {
          returnValue = '';
          if (
            (window as any)[gridOptions.columnHeader[cellCounter].formatter] ||
            gridOptions.columnHeader[cellCounter].formatter === 'trimOnOverflowAndShowToolTip'
          ) {
            switch (gridOptions.columnHeader[cellCounter].formatter) {
              case 'parseCustomBarChart': {
                let count = 0;
                let total = 0;
                let maxValue = 1;
                let current = 0;
                const sFieldName = gridOptions.columnHeader[cellCounter].name;
                if (gridOptions.data.length) {
                  total = gridOptions.data.length;
                  for (count = 0; count < total; count++) {
                    current = parseFloat(gridOptions.data[count][sFieldName].toString());
                    if (!isGroupedRow || gridOptions.data[rowPosition].groupHeaderName === gridOptions.data[count].groupHeaderName) {
                      if (!isNaN(current)) {
                        if (maxValue < current) {
                          maxValue = current;
                        }
                      }
                    }
                  }
                }
                const formatterOptions = {
                  maxValue: maxValue,
                  field: sFieldName,
                  numberFormatter: gridOptions.columnHeader[cellCounter].chartValueFormatter,
                  fCell: cell.style.width || GridConstants.sDefaultWidth,
                };

                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  formatterOptions
                );
                break;
              }
              case 'parseDealValue':
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name]
                );
                break;
              case 'parsePastDueDeals':
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                  gridOptions.dataConfiguration.maxConfig
                );
                break;
              case 'parseOpportunitiesTrack':
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.dataConfiguration.maxConfig,
                  cell.style.width || ''
                );
                break;
              case 'parseOpportunitiesMissing':
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.dataConfiguration.maxConfig,
                  cell.style.width || ''
                );
                break;
              case 'parseQuotaCoverage':
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.dataConfiguration.maxConfig
                );
                break;
              case 'calculatePercentByTotal': {
                const formatterOptions = {
                  maxConfig: gridOptions.dataConfiguration.maxConfig,
                  field: gridOptions.endRow.data,
                  fCell: cell.style.width || '',
                  cellCounter: rowPosition,
                  barColor: gridOptions.columnHeader[2].barColors[rowPosition],
                  barColors: gridOptions.columnHeader[2].barColors,
                  dataSeries: gridOptions.data,
                };
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.gridName,
                  gridOptions.columnHeader[cellCounter].name,
                  formatterOptions
                );

                break;
              }
              case 'customPipelineBar':
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition],
                  gridOptions.data,
                  gridOptions.endRow.data
                );
                break;

              default:
                const formatterOptions = {
                  maxConfig: gridOptions.dataConfiguration.maxConfig,
                  field: gridOptions.endRow.data,
                  fCell: cell.style.width || '',
                  cellCounter: rowPosition,
                  headerProperties: gridOptions.columnHeader[cellCounter],
                  dataSeries: gridOptions.data,
                  oInPlaceGridData: gridOptions.inPlaceGrid,
                  customSecondaryFormatter: gridOptions.dataConfiguration.customSecondaryFormatter,
                  stackedBarConfig: gridOptions.dataConfiguration.stackedBar,
                };
                returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                  gridOptions.data[rowPosition][gridOptions.columnHeader[cellCounter].name],
                  gridOptions.data[rowPosition],
                  gridOptions.gridName,
                  gridOptions.columnHeader[cellCounter].name,
                  formatterOptions
                );
                break;
            }
          }
        }
        if (gridOptions.columnHeader[cellCounter].category === 'ImageUrl') {
          let img = document.createElement('img');
          img.src = returnValue.toString();
          img.style.height = '100%';
          cell.appendChild(img);
          var count,
            tooltipText = '';
          for (count = 0; count < gridOptions.tooltipColumnHeader.length; count++) {
            let tooltipTitle = gridOptions.tooltipColumnHeader[count];
            tooltipText += tooltipTitle + ': ' + gridOptions.tooltipData[rowPosition][tooltipTitle] + '\n';
          }
          img.title = tooltipText;
        } else if (gridOptions.columnHeader[cellCounter].category === 'WebUrl') {
          let link = document.createElement('a');
          link.textContent = returnValue.toString();
          cell.appendChild(link);
        } else {
          $(cell).text(returnValue.toString());
        }
      }
    }
  }

  if (
    !gridOptions.serverGrid.enabled &&
    gridOptions.fixedHeaderEnd &&
    parseInt(gridOptions.fixedHeaderEnd) <= cellCounter &&
    gridOptions.pagination.paginate &&
    gridOptions.totalPages &&
    (endIndex - startIndex < gridOptions.pagination.maxRows || 1 === gridOptions.pagination.iLast)
  ) {
    const leftGrid = document.querySelector('#' + gridOptions.gridName + ' .LeftGrid') as HTMLDivElement;
    const rightGrid = document.querySelector('#' + gridOptions.gridName + ' .RightGrid') as HTMLDivElement;
    rightGrid.style.height = leftGrid.clientHeight - 38 + 'px';
    gridOptions.pagination.iLast *= -1;
  }

  if (gridOptions.endRow.enableEndRow) {
    // Use only data array
    let cellCounter = 0;
    let cellCounterRight = 0;
    gridOptions.endRow.endRowPosition = gridOptions.endRow.endRowPosition;
    if (
      !isNaN(gridOptions.endRow.endRowPosition) &&
      gridOptions.endRow.endRowPosition >= -1 &&
      gridOptions.endRow.endRowPosition < gridOptions.data.length
    ) {
      const row = gridOptions.tblBody.insertRow(gridOptions.endRow.endRowPosition);
      let rowRight: HTMLTableRowElement;
      if (gridOptions.fixedHeaderEnd) {
        rowRight = gridOptions.tblBodyRight.insertRow(gridOptions.endRow.endRowPosition);
        utility.addClass(rowRight, gridOptions.endRow.className);
      }

      utility.addClass(row, gridOptions.endRow.className);
      for (cellCounter = 0; cellCounter < numberOfColumns; cellCounter++) {
        let cell: HTMLTableCellElement;
        if (gridOptions.fixedHeaderEnd && parseInt(gridOptions.fixedHeaderEnd) <= cellCounter) {
          const cell = rowRight.insertCell(cellCounterRight++);
        } else {
          cell = row.insertCell(cellCounter++);
        }

        // Doing this to avoid overlapping of last column values and the scroll bar
        if (gridOptions.columnHeader[cellCounter].noOverlap) {
          if (cellCounter === numberOfColumns - 1) {
            cell.style.paddingRight = String(20) + 'px';
            utility.addClass(cell, 'noOverlap');
          }
        }
        if (gridOptions.columnHeader[cellCounter].style) {
          utility.applyStyleToObject(cell, gridOptions.columnHeader[cellCounter].style);
        }
        utility.addClass(cell, 'jsonGridRow');
        utility.addClass(cell, 'GridEndRow');
        cell.style.textAlign = gridOptions.columnHeader[cellCounter].align;

        let returnValue: DataColumn[string];
        if (gridOptions.endRow.includeFormatters && gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]) {
          switch (gridOptions.columnHeader[cellCounter].formatter) {
            case 'parseDealValue':
              returnValue = (window as any)[gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]](
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                gridOptions.endRow.data,
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name]
              );
              break;
            case 'parsePastDueDeals':
              returnValue = (window as any)[gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]](
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                gridOptions.endRow.data,
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name]
              );
              break;
            case 'parseOpportunitiesTrack':
              returnValue = (window as any)[gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]](
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                gridOptions.endRow.data,
                gridOptions.dataConfiguration.maxConfig,
                cell.style.width || GridConstants.sDefaultWidth
              );
              break;
            case 'parseOpportunitiesMissing':
              returnValue = (window as any)[gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]](
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                gridOptions.endRow.data,
                gridOptions.dataConfiguration.maxConfig,
                cell.style.width || GridConstants.sDefaultWidth
              );
              break;
            case 'parseQuotaCoverage':
              returnValue = (window as any)[gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]](
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                gridOptions.endRow.data,
                gridOptions.dataConfiguration.maxConfig
              );
              break;
            case 'customPipelineBar':
              returnValue = (window as any)[gridOptions.columnHeader[cellCounter].formatter](
                gridOptions.endRow.data,
                gridOptions.data,
                gridOptions.endRow.data
              );
              break;
            default:
              returnValue = (window as any)[gridOptions.endRow.includeFormatters[gridOptions.columnHeader[cellCounter].name]](
                gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name],
                gridOptions.endRow.data,
                gridOptions.gridName,
                gridOptions.columnHeader[cellCounter].name
              );
              break;
          }
        } else {
          returnValue = gridOptions.endRow.data[gridOptions.columnHeader[cellCounter].name] || GridConstants.sNA;
        }

        $(cell).text(returnValue.toString());
      }
      if (gridOptions) {
        // Add 15px split row above total row
        if (
          'undefined' !== typeof gridOptions.endRow.isSplitRowEnabled &&
          gridOptions.endRow.isSplitRowEnabled &&
          (window as any)[gridOptions.endRow.splitRowFormatter]
        ) {
          (window as any)[gridOptions.endRow.splitRowFormatter](gridOptions.container, gridOptions.data.length);
        }
      }
    }
  }

  gridOptions.data = originalGridData;
}

function CreateHTMLTableWithHeader(gridOptions: IGridOptions) {
  const tHead = gridOptions.tblHead,
    numberOfHeaderColumns = gridOptions.columnHeader.length,
    iParentHeaderCount = gridOptions.headerTemplate.length;

  let cellCounter = 0;
  let cellCounterRight = 0;

  let headerRight: HTMLTableSectionElement;
  let oRowRight: HTMLTableRowElement;
  let row = tHead.insertRow(-1);
  if (gridOptions.fixedHeaderEnd) {
    headerRight = gridOptions.tblHeadRight;
    oRowRight = headerRight.insertRow(-1);
  }
  for (let i = 0; i < iParentHeaderCount; i++) {
    let cell: HTMLTableCellElement;
    if (gridOptions.headerTemplate[i] && gridOptions.headerTemplate[i].dataID) {
      const currentGroup = gridOptions.headerTemplate[i].dataID.split(',');
      const groupEnd = currentGroup[currentGroup.length - 1];
      if (gridOptions.fixedHeaderEnd && parseInt(gridOptions.fixedHeaderEnd) < parseInt(groupEnd)) {
        cell = oRowRight.insertCell(cellCounterRight++);
      } else {
        cell = row.insertCell(cellCounter++);
      }
    }
    cell.setAttribute('colspan', (gridOptions.headerTemplate[i].colSpan || 1).toString());
    $(cell).text(gridOptions.headerTemplate[i].columnText || '');
    cell.setAttribute('dataID', gridOptions.headerTemplate[i].dataID || 'parent');
    cell.setAttribute('onclick', gridOptions.headerTemplate[i].onclick || '');
    utility.addClass(cell, 'jsonGridParentHeader');
    if (gridOptions.headerTemplate[i].headerClassName) {
      utility.addClass(cell, gridOptions.headerTemplate[i].headerClassName);
    }
    utility.applyStyleToObject(cell, gridOptions.headerTemplate[i].style || {});
  }

  if (iParentHeaderCount) {
    row = tHead.insertRow(-1);
    if (gridOptions.fixedHeaderEnd) {
      oRowRight = headerRight.insertRow(-1);
    }
  }
  cellCounter = 0;
  cellCounterRight = 0;
  for (let i = 0; i < numberOfHeaderColumns; i += 1) {
    let cell: HTMLTableCellElement;
    if (gridOptions.columnHeader[i].roles['Values']) {
      if (gridOptions.fixedHeaderEnd && parseInt(gridOptions.fixedHeaderEnd) <= i) {
        cell = oRowRight.insertCell(cellCounterRight++);
      } else {
        cell = row.insertCell(cellCounter++);
      }
      cell.setAttribute('id', gridOptions.columnHeader[i].id || 'jsonGridHeader_' + i);
      utility.addClass(cell, gridOptions.columnHeader[i].headerClassName);
      utility.addClass(cell, 'jsonGridHeader');
      if (gridOptions.columnHeader[i].style) {
        if (i === numberOfHeaderColumns - 1) {
          cell.style.width = gridOptions.scrolling.enabled
            ? gridOptions.tblBody.rows[0].cells[i].clientWidth - 8 + 'px'
            : gridOptions.columnHeader[i].style.width;
        } else {
          cell.style.width = gridOptions.scrolling.enabled
            ? gridOptions.tblBody.rows[0].cells[i].clientWidth - 30 + 'px'
            : gridOptions.columnHeader[i].style.width;
        }
        cell.style.textAlign = gridOptions.columnHeader[i].style.textAlign;
      }
      if (i + 1 === numberOfHeaderColumns) {
        cell.style.paddingRight = 10 + 'px';
      }
      var regex = /[!\"#$%&'\(\)\*\+,\.\/:;<=>\?\@\[\\\]\^`\{\|\}~ ]/g;
      // Add sorting functionality
      if (gridOptions.columnHeader[i].sortable) {
        cell.onclick = () => sortJsonGrid(cell, gridOptions.gridName, gridOptions.columnHeader[i].name);
        cell.style.cursor = 'pointer';
        if (gridOptions.columnHeader[i].name === gridOptions.gridSort.sortBy) {
          if (gridOptions.gridSort.sortOrder === 'asc') {
            cell.setAttribute('sortOrder', 'desc');
          } else {
            cell.setAttribute('sortOrder', 'asc');
          }
          if (gridOptions.hiddenContainer) {
            gridOptions.hiddenContainer.setAttribute('data-sortKey', gridOptions.columnHeader[i].sortKey);
          }
        } else {
          cell.setAttribute('sortOrder', gridOptions.gridSort.sortOrder);
        }
        cell.setAttribute('sortKey', gridOptions.columnHeader[i].sortKey);
      } else {
        cell.style.cursor = 'default';
      }
      if (!gridOptions.gridSort.sortBy && gridOptions.columnHeader[i].sortable) {
        gridOptions.gridSort.sortBy = gridOptions.columnHeader[i].name;
      }
      var span = document.createElement('span');
      utility.addClass(span, 'ColumnText');
      span.textContent = gridOptions.columnHeader[i].columnText;
      cell.appendChild(span);
      if (gridOptions.columnHeader[i].name === gridOptions.gridSort.sortBy && gridOptions.columnHeader[i].sortable) {
        utility.addClass(cell, 'jsonGridHeaderAlternate');
        utility.addClass(document.querySelectorAll('#' + gridOptions.container + ' .SortIndicator'), 'itemHide');
        var spanInner = document.createElement('span');
        var spanOuter = document.createElement('span');
        spanOuter.appendChild(spanInner);
        utility.addClass(spanOuter, 'SortIndicator sort' + gridOptions.columnHeader[i].name.replace(regex, '_') + 'Hand');
        cell.appendChild(spanOuter);
        if (gridOptions.gridSort.sortOrder === 'asc') {
          utility.addClass(spanInner, 'asc');
        } else {
          utility.addClass(spanInner, 'desc');
        }
      } else if (gridOptions.columnHeader[i].sortable) {
        var spanInner = document.createElement('span');
        utility.addClass(spanInner, 'asc');
        var spanOuter = document.createElement('span');
        spanOuter.appendChild(spanInner);
        utility.addClass(spanOuter, 'SortIndicator itemHide sort' + gridOptions.columnHeader[i].name.replace(regex, '_') + 'Hand');
        cell.appendChild(spanOuter);
      }
    }
  }
}

function CreateHTMLTable(gridOptions: IGridOptions): HTMLDivElement | HTMLTableElement {
  let gridContainer: HTMLDivElement;
  if (gridOptions.fixedHeaderEnd) {
    gridContainer = document.createElement('div');
    gridContainer.setAttribute('id', gridOptions.gridName + '_HeaderParent');
  }
  const grid = document.createElement('table');
  grid.setAttribute('id', gridOptions.gridName);
  if (!gridOptions.container) {
    grid.setAttribute('class', 'jsonGrid');
  } else {
    grid.setAttribute('class', 'InnerJsonGrid');
  }
  if (JSON.stringify(gridOptions.style) !== '{}') {
    utility.applyStyleToObject(grid, gridOptions.style);
  }
  if (gridOptions.fixedHeaderEnd) {
    const containerDiv1 = document.createElement('div');
    utility.addClass(containerDiv1, 'LeftGrid');
    containerDiv1.appendChild(grid);
    gridContainer.appendChild(containerDiv1);
    gridOptions.containerObject.appendChild(gridContainer);
    if (gridOptions.columnHeader.length === parseInt(gridOptions.fixedHeaderEnd)) {
      containerDiv1.style.width = '100%';
    }
    const rightGrid = document.createElement('table');
    rightGrid.setAttribute('id', gridOptions.gridName + '_right');
    utility.addClass(rightGrid, 'jsonGrid');
    utility.applyStyleToObject(rightGrid, gridOptions.style);
    const containerDiv2 = document.createElement('div');
    utility.addClass(containerDiv2, 'RightGrid');
    containerDiv2.appendChild(rightGrid);
    gridContainer.appendChild(containerDiv2);
  } else {
    gridOptions.containerObject.appendChild(grid);
  }

  return gridOptions.fixedHeaderEnd ? gridContainer : grid;
}

function CreateLegends(oGridConfiguration: IGridOptions) {
  var oLegendContainer = document.createElement('div'),
    oLegendSectionCover = document.createElement('div'),
    oLegendTitleSection = document.createElement('div'),
    oLegendTitleLabel = document.createElement('div'),
    oLegendSection = document.createElement('div'),
    oLegendSpace,
    oLegendLabel,
    oLegendIndicator,
    oLegendDivision,
    iIterator = 0;
  utility.addClass(oLegendContainer, 'LegendContainer');
  utility.addClass(oLegendSection, 'LegendSection');
  if (oGridConfiguration.legends && oGridConfiguration.legends.legendTemplate) {
    utility.applyStyleToObject(oLegendContainer, oGridConfiguration.legends.containerStyle);
    const oData = oGridConfiguration.legends.legendTemplate;
    for (iIterator = 0; iIterator < oData.length; iIterator++) {
      oLegendLabel = document.createElement('div');
      oLegendLabel.innerText = oData[iIterator].label;
      oLegendIndicator = document.createElement('div');
      oLegendDivision = document.createElement('div');
      oLegendIndicator.appendChild(oLegendDivision);
      utility.applyStyleToObject(oLegendDivision, oData[iIterator].indicatorStyle);
      utility.applyStyleToObject(oLegendLabel, oData[iIterator].labelStyle);
      if (!oGridConfiguration.legends.labelFirst) {
        oLegendSection.appendChild(oLegendIndicator);
        oLegendSection.appendChild(oLegendLabel);
      } else {
        oLegendSection.appendChild(oLegendLabel);
        oLegendSection.appendChild(oLegendIndicator);
      }
      oLegendSpace = document.createElement('div');
      oLegendSection.appendChild(oLegendSpace);
      utility.addClass(oLegendLabel, 'LegendLabel');
      utility.addClass(oLegendSpace, 'LegendSpace');
      utility.addClass(oLegendIndicator, 'LegendIndicator');
      utility.applyStyleToObject(oLegendSpace, oGridConfiguration.legends.separationStyle);
    }
    oLegendTitleLabel.innerText = oGridConfiguration.legends.legendTitle || '';
    utility.applyStyleToObject(oLegendTitleLabel, oGridConfiguration.legends.titleStyle);
    oLegendTitleSection.appendChild(oLegendTitleLabel);
    oGridConfiguration.containerObject.appendChild(oLegendTitleSection);
    oLegendSectionCover.appendChild(oLegendSection);
    oLegendContainer.appendChild(oLegendSectionCover);
  }
  oGridConfiguration.containerObject.appendChild(oLegendContainer);
}

function JsonGrid(gridOptions: IGridOptions) {
  let containerObject: HTMLElement;
  if (!gridOptions.container) {
    containerObject = document.getElementById(gridOptions.containerName);

    // Append data if grid already exists
    if (document.getElementById(gridOptions.gridName)) {
      appendDataToGrid(gridOptions);
      return true;
    }
  } else {
    containerObject = gridOptions.container;
  }
  if (gridOptions.data.length <= 0) {
    return false;
  }

  if (containerObject) {
    const newGridOptions: IGridOptions = {
      containerObject: containerObject,

      containerName: '',
      gridName: '',
      hiddenName: '',

      data: [],
      headerTemplate: [],
      columnHeader: [],
      style: {},
      altRowColor: '',
      gridSort: {
        sortBy: '',
        sortOrder: 'asc',
      },
      serverGrid: {
        enabled: false,
        totalPages: 2,
        currentIndex: 1,
        sendRequestFunction: null,
      },
      inPlaceGrid: {
        enableInPlaceGrid: false,
        disableHeader: false,
        parentContainer: '',
        level: '',
        enableRowInsert: false,
      },
      viewRecords: true,
      pagination: {
        maxRows: 0,
        retainPageOnSort: true,
        iLast: -1,
        paginate: false,
      },
      scrolling: {
        enabled: false,
        scrollStyle: {},
      },
      cellSpacing: 0,
      cellPadding: 0,
      rows: {
        alternate: true,
        rowClassName: '',
      },
      endRow: {
        enableEndRow: false,
        isTotalRow: false,
        includeFormatters: {},
        columnsExcluded: [],
        data: {},
        endRowPosition: -1,
        className: '',
        isSplitRowEnabled: false,
        splitRowFormatter: '',
      },
      legends: {
        enableLegends: false,
        labelFirst: false,
        separationStyle: {},
        containerStyle: {},
        legendTemplate: [],
      },
      groupedRows: false,
      groupedRowHeader: {
        groupHeaderName: [],
        data: [],
      },
      fixedHeaderEnd: null,
      dataConfiguration: {
        calculateMaximum: false,
        columnsIncluded: [],
        maxConfig: {},
        includeEndRow: false,
        useAbsolutes: [],
        stackedBar: {
          enabled: false,
          stackedColumns: [],
          color: [],
          colorMapping: {
            hasMultiColoredBars: false,
            mappingColumn: '',
            colorMap: [],
          },
          hasRelativeRows: true,
          relateByColumn: '',
          displayRelateByColumn: false,
          className: '',
        },
        customSecondaryFormatter: '',
      },
      callBackFunc: () => {},
      fetchData: {
        enabled: false,
        getNextPage: () => {},
        hasMoreData: false,
      },
    };

    for (const attribute in gridOptions) {
      if (!gridOptions.hasOwnProperty(attribute)) continue;

      const prop = gridOptions[attribute as keyof IGridOptions];
      // Merge and clone for first level attributes
      if ('object' === typeof prop && !(prop instanceof Array)) {
        for (let attributeObject in prop) {
          (newGridOptions[attribute as keyof IGridOptions][attributeObject as keyof typeof prop] as any) = utility.clone(
            prop[attributeObject as keyof typeof prop]
          );
        }
      } else {
        (newGridOptions[attribute as keyof IGridOptions] as any) = utility.clone(prop);
      }
    }

    if (newGridOptions.fixedHeaderEnd) {
      newGridOptions.scrolling.enabled = false;
    }
    if (newGridOptions.scrolling.enabled) {
      newGridOptions.pagination.paginate = false;
    }
    if (!newGridOptions.pagination.paginate) {
      newGridOptions.pagination.maxRows = newGridOptions.data.length;
    }

    if (newGridOptions.legends.enableLegends) {
      CreateLegends(newGridOptions);
    }
    newGridOptions.currentPage = 0;
    if (newGridOptions.pagination.maxRows > 0) {
      if (newGridOptions.serverGrid.enabled) {
        newGridOptions.totalPages = newGridOptions.serverGrid.totalPages - 1;
      } else {
        newGridOptions.totalPages = Math.ceil(newGridOptions.data.length / newGridOptions.pagination.maxRows) - 1;
      }
    } else {
      newGridOptions.totalPages = 0;
    }

    for (let loopCounter = 0; loopCounter < newGridOptions.columnHeader.length; loopCounter += 1) {
      if (
        !(
          newGridOptions.data[0].hasOwnProperty([newGridOptions.columnHeader[loopCounter].name].toString()) ||
          'DUMMY' === newGridOptions.columnHeader[loopCounter].name
        )
      ) {
        return false;
      }

      // Add color configuration for custom bar chart column
      if (
        newGridOptions.columnHeader[loopCounter].formatter &&
        newGridOptions.columnHeader[loopCounter].formatter === 'parseCustomBarChart'
      ) {
        const iTotal = newGridOptions.data.length;
        if (newGridOptions.columnHeader[loopCounter].chartColor && iTotal === newGridOptions.columnHeader[loopCounter].chartColor.length) {
          for (let i = 0; i < iTotal; i++) {
            let style = newGridOptions.data[i].colorStyle as ColumnColorStyle;
            if (!style || typeof style !== 'object') {
              style = {
                columnName: [],
                color: [],
              };
              newGridOptions.data[i].colorStyle = style;
            }

            if (!style.columnName) style.columnName = [];
            if (!style.color) style.color = [];

            style.columnName.push(newGridOptions.columnHeader[loopCounter].name);
            style.color.push(newGridOptions.columnHeader[loopCounter].chartColor[i]);
          }
        }
      }
    }

    // Create Hidden grid to store parameters for server side grid.
    // Fork for client/server type grid.
    if (newGridOptions.serverGrid.enabled) {
      createHiddenChunk(newGridOptions);

      // Add scroll handler if grid supports scrolling and Server Side is enabled.
      if (newGridOptions.scrolling.enabled) {
        newGridOptions.containerObject.addEventListener(
          'scroll',
          function (event) {
            handleGridScroll(event.currentTarget as HTMLElement);
          },
          false
        );
      }
    } else {
      if (newGridOptions.gridSort.sortBy) {
        if (!newGridOptions.groupedRows) {
          if (newGridOptions.gridSort.sortOrder.toLowerCase() === 'asc') {
            newGridOptions.data.sort(utility.sortBy(newGridOptions.gridSort.sortBy, true, newGridOptions.gridSort.sortType));
          }
          if (newGridOptions.gridSort.sortOrder.toLowerCase() === 'desc') {
            newGridOptions.data.sort(utility.sortBy(newGridOptions.gridSort.sortBy, false, newGridOptions.gridSort.sortType));
          }
        } else {
          if (newGridOptions.groupedRowHeader && newGridOptions.groupedRowHeader.groupHeaderName) {
            // Sort groups separately and then combine the data
            if (newGridOptions.gridSort.sortOrder.toLowerCase() === 'asc') {
              newGridOptions.data = sortDataWithinGroup(
                newGridOptions,
                newGridOptions.gridSort.sortBy,
                true,
                newGridOptions.gridSort.sortType
              );
            } else {
              newGridOptions.data = sortDataWithinGroup(
                newGridOptions,
                newGridOptions.gridSort.sortBy,
                false,
                newGridOptions.gridSort.sortType
              );
            }
          }
        }
      }
    }

    const columnHeaderLength = newGridOptions.columnHeader.length;
    const dataLength = newGridOptions.data.length;
    if (newGridOptions.endRow.isTotalRow) {
      for (let iColumnIterator = 0; iColumnIterator < columnHeaderLength; iColumnIterator++) {
        for (let iIterator = 0; iIterator < dataLength; iIterator++) {
          if (
            newGridOptions.endRow.columnsExcluded &&
            newGridOptions.endRow.columnsExcluded.indexOf(newGridOptions.columnHeader[iColumnIterator].name) < 0
          ) {
            let endRowValue = this.gridOptions.endRow.data[this.gridOptions.columnHeader[iColumnIterator].name];
            if ('undefined' === typeof endRowValue) {
              endRowValue = 0;
            }
            if (typeof endRowValue !== 'number') {
              newGridOptions.endRow.data[newGridOptions.columnHeader[iColumnIterator].name] = GridConstants.sNA;
              break;
            }

            let currentValue = parseFloat(newGridOptions.data[iIterator][newGridOptions.columnHeader[iColumnIterator].name].toString());
            if (isNaN(currentValue)) {
              currentValue = 0;
            }
            newGridOptions.endRow.data[newGridOptions.columnHeader[iColumnIterator].name] = endRowValue + currentValue;
          }
        }
      }
    }
    if (newGridOptions.dataConfiguration.calculateMaximum) {
      calculateMinMax(newGridOptions, 0, dataLength);
    }
    if (newGridOptions.inPlaceGrid?.enableInPlaceGrid !== true) {
      const gridObject = CreateHTMLTable(newGridOptions);

      let gridTable: HTMLTableElement;
      if (newGridOptions.fixedHeaderEnd) {
        gridTable = gridObject.childNodes[0].childNodes[0] as HTMLTableElement;

        const gridTableRight = gridObject.childNodes[1].childNodes[0] as HTMLTableElement;
        newGridOptions.tblHeadRight = gridTableRight.createTHead();
        newGridOptions.tblFootRight = gridTableRight.createTFoot();
        const tBody = document.createElement('tbody');
        const tableBody = document.createElement('tbody');
        gridTableRight.appendChild(tableBody);
        newGridOptions.tblBodyRight = tableBody;
      } else {
        gridTable = gridObject as HTMLTableElement;
      }

      newGridOptions.tblHead = gridTable.createTHead();
      newGridOptions.tblFoot = gridTable.createTFoot();
      const tBody = document.createElement('tbody');
      gridTable.appendChild(tBody);
      newGridOptions.tblBody = tBody;

      newGridOptions.gridObject = gridObject;
    }

    if (0 === newGridOptions.totalPages) {
      $('.DataDiv').css('height', '100%');
    } else {
      $('.DataDiv').css('height', 'calc(100% - 35px)');
    }
    // Create Pagination if grid supports pagination
    if (newGridOptions.pagination.paginate) {
      CreateHTMLTableWithHeader(newGridOptions);
      CreateHTMLTableRow(newGridOptions);
      if (newGridOptions.totalPages) {
        CreatePaginationControl(newGridOptions);
      }
      if (newGridOptions.fetchData.enabled) {
        CreateFetchControl(newGridOptions);
      }
    } else if (newGridOptions.scrolling.enabled) {
      CreateHTMLTableRow(newGridOptions);
      CreateHTMLTableWithHeader(newGridOptions);
      const margin = newGridOptions.tblHead.clientHeight;
      newGridOptions.tblHead.style.marginTop = '-' + margin + 'px';
      newGridOptions.containerObject.style.marginTop = margin + 'px';
      newGridOptions.gridObject.style.width = newGridOptions.containerObject.clientWidth + 'px';
      utility.addClass(newGridOptions.tblHead, 'jsonScrollHeader');
      utility.addClass(newGridOptions.containerObject, 'jsonScrollContainer');
    } else {
      if (newGridOptions.inPlaceGrid.disableHeader === false) {
        CreateHTMLTableWithHeader(newGridOptions);
      }
      CreateHTMLTableRow(newGridOptions);
    }
    gridObjects[newGridOptions.gridName] = newGridOptions;
  }

  //Pagination issue fix
  var selectedElement = document.getElementsByClassName('ListOptionContainer')[0];
  if (undefined !== selectedElement) {
    selectedElement.addEventListener('change', function (e) {
      jumpTo(0);
    });
  }

  selectedElement = document.getElementsByClassName('ListOptionContainer')[1];
  if (undefined !== selectedElement) {
    selectedElement.addEventListener('change', function (e) {
      jumpTo(1);
    });
  }
  return true;
}

function jumpTo(index: number) {
  const listOptionsContainer = document.getElementsByClassName('ListOptionContainer');
  if (listOptionsContainer && listOptionsContainer[index] && (listOptionsContainer[index] as HTMLSelectElement).options) {
    const selectElement = listOptionsContainer[index] as HTMLSelectElement;
    (window as any).pageId = selectElement.options.selectedIndex + 1;

    let totalPages = (document.getElementsByClassName('totalPagesLabel')[index] as HTMLLabelElement).innerText;
    totalPages = totalPages.substring(totalPages.indexOf('of') + 2, totalPages.length).trim();

    let elemJumpTo = selectElement.options[selectElement.options.selectedIndex];
    if ((window as any).pageId > totalPages) {
      (window as any).pageId = totalPages;
      selectElement.value = (window as any).pageId;
      elemJumpTo = selectElement.options[(window as any).pageId - 1];
    }
    newRecords(elemJumpTo, $(selectElement).parents('table').attr('id'));
  } else {
    (window as any).pageId = (listOptionsContainer[0] as HTMLInputElement).value;
  }
}

// Function to create and update hidden chunk (this will be called only in case of server side grid)
function createHiddenChunk(gridOptions: IGridOptions) {
  const container = gridOptions.container;
  let hiddenContainer = document.getElementById(container + '_hidden');
  if (hiddenContainer) {
    gridOptions.currentPage = parseInt(hiddenContainer.getAttribute('data-currentPage'));
    gridOptions.totalPages = parseInt(hiddenContainer.getAttribute('data-totalPages'));
    gridOptions.pagination.maxRows = parseInt(hiddenContainer.getAttribute('data-maxRows'));
    gridOptions.gridSort.sortBy = hiddenContainer.getAttribute('data-sortBy');
    gridOptions.gridSort.sortOrder = hiddenContainer.getAttribute('data-sortOrder') === 'desc' ? 'desc' : 'asc';
  } else {
    hiddenContainer = document.createElement('div');
    hiddenContainer.id = container + '_hidden';
    hiddenContainer.setAttribute('data-totalPages', gridOptions.totalPages.toString());
    hiddenContainer.setAttribute('data-currentPage', gridOptions.currentPage.toString());
    hiddenContainer.setAttribute('data-maxRows', gridOptions.pagination.maxRows.toString());
    hiddenContainer.setAttribute('data-sortBy', gridOptions.gridSort.sortBy);
    hiddenContainer.setAttribute('data-sortOrder', gridOptions.gridSort.sortOrder);
    hiddenContainer.setAttribute('data-sent', '0'); // To check if response is received or not in case of scrollable server side grid
    utility.addClass(hiddenContainer, 'Hidden');
    gridOptions.containerObject.parentElement.appendChild(hiddenContainer);
    gridOptions.hiddenContainer = hiddenContainer;
  }
}

// Function to send service request in case of server side grid
function callService(gridOptions: IGridOptions) {
  const oHiddenContainer = document.getElementById(gridOptions.hiddenName);
  const callBack = gridOptions.serverGrid.sendRequestFunction;

  // Properties to be sent in request
  const parameters = {
    maxRows: gridOptions.pagination.maxRows,
    sortOrder: oHiddenContainer.getAttribute('data-sortOrder'),
    sortBy: oHiddenContainer.getAttribute('data-sortBy'),
    sortKey: oHiddenContainer.getAttribute('data-sortKey'),
    startIndex: parseInt(oHiddenContainer.getAttribute('data-currentPage')) + 1,
    gridContainer: gridOptions.container,
  };
  callBack(parameters);
}

// Function to add scroll handler
function handleGridScroll(currentElement: HTMLElement) {
  const gridOptions = gridObjects[currentElement.id + '_Grid'];
  if (gridOptions) {
    const hiddenContainer = document.getElementById(gridOptions.hiddenName);
    if (hiddenContainer && currentElement.scrollTop === currentElement.scrollHeight - currentElement.offsetHeight) {
      const requestPending = hiddenContainer.getAttribute('data-sent');
      const maxPageNumber = parseInt(hiddenContainer.getAttribute('data-totalPages')) || 0;
      const currentPage = (parseInt(hiddenContainer.getAttribute('data-currentPage')) || 0) + 1;
      if ('1' !== requestPending && currentPage <= maxPageNumber) {
        hiddenContainer.setAttribute('data-currentPage', currentPage.toString());
        hiddenContainer.setAttribute('data-sent', '1');
        callService(gridOptions);
      }
    }
  }
}

// Function to append data to existing grid on scroll
function appendDataToGrid(gridOptions: IGridOptions) {
  var hiddenContainer = document.getElementById(gridOptions.hiddenName);
  if (gridObjects[gridOptions.gridName]) {
    const gridConfigurationOptions = gridObjects[gridOptions.gridName];
    gridConfigurationOptions.data = gridOptions.data;
    hiddenContainer.setAttribute('data-sent', '0');
    CreateHTMLTableRow(gridConfigurationOptions);
  }
}

// Function to calculate max and min
function calculateMinMax(gridOptions: IGridOptions, iStartIndex: number, iEndIndex: number) {
  let max, currentValue, min;
  const columnIncludedLength = gridOptions.dataConfiguration.columnsIncluded.length;
  for (let i = 0; i < columnIncludedLength; i++) {
    const nCurrentColumn = gridOptions.dataConfiguration.columnsIncluded[i];
    (max = 0), (min = 0);
    for (let iIterator = iStartIndex; iIterator < iEndIndex; iIterator++) {
      currentValue = Number(gridOptions.data[iIterator][nCurrentColumn]);
      if (isNaN(currentValue)) {
        currentValue = 0;
      }
      if (gridOptions.dataConfiguration.useAbsolutes.indexOf(nCurrentColumn) > -1) {
        if (Math.abs(currentValue) > max) {
          max = Math.abs(currentValue);
        }
      } else {
        if (currentValue > max) {
          max = currentValue;
        }
        if (currentValue < min) {
          min = currentValue;
        }
      }
    }
    if (gridOptions.dataConfiguration.includeEndRow && gridOptions.endRow.enableEndRow) {
      currentValue = Number(gridOptions.endRow.data[nCurrentColumn]);
      if (isNaN(currentValue)) {
        currentValue = 0;
      }
      if (gridOptions.dataConfiguration.useAbsolutes.indexOf(nCurrentColumn) > -1) {
        if (Math.abs(currentValue) > max) {
          max = Math.abs(currentValue);
        }
      } else {
        if (currentValue > max) {
          max = currentValue;
        }

        if (currentValue < min) {
          min = currentValue;
        }
      }
    }
    gridOptions.dataConfiguration.maxConfig[nCurrentColumn] = max;
    gridOptions.dataConfiguration.maxConfig[nCurrentColumn + '_Min'] = min;
  }
  return gridOptions;
}

function getChildPosition(childNode: Element, parentNode: HTMLTableSectionElement) {
  let index = -1,
    iCount,
    iTotal = parentNode.querySelectorAll(childNode.tagName).length,
    aTrs = parentNode.querySelectorAll(childNode.tagName);
  for (iCount = 0; iCount < iTotal; iCount++) {
    var oCurrentNode = aTrs[iCount];
    if (childNode === oCurrentNode) {
      index = iCount;
      return index;
    }
  }
}
