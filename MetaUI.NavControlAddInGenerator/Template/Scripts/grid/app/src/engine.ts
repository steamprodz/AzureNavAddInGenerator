import {
    ODataDataSource,
    IControlInfo,
    InMemoryCacheService,
    BasicUser,
    BasicAuthenticationProvider,
    UrlSettingsProvider,
    DataSource,
    FakeSubject,
    ControlStyleThemeProvider,
    Theme
} from '@meta-ui/core';

import {
    Grid
} from '@meta-ui/skeleton';
import { Subscription, of } from 'rxjs';
import 'core-js/es/array';

class RemoteResourceThemeProvider implements ControlStyleThemeProvider {
    public hostUrl: string;
    constructor(hostUrl: string) {
        this.hostUrl = hostUrl;
    }

    resolveTheme(theme: string): Promise<Theme> {
        let themeLink;

        switch (theme) {
            case 'default':
                themeLink = this.hostUrl + '/client-default.css';
                break;
            case 'BC':
                themeLink = this.hostUrl + '/client-BC.css';
                break;
        }

        return of({ name: theme, files: [{ link: themeLink, mimeType: 'css', order: 0 }] }).toPromise();
    }
}

let resizeControlInterval;
(window as any).initExtension = (config) => {
    window.metaUI.events.onInfrastructureReady.subscribe({
        next: () => {
            let rootParentWindow: Window;
            if (resizeControlInterval) {
                clearInterval(resizeControlInterval);
            }
            if (rootParentWindow) {
                resizeControlInterval = setInterval(() => rootParentWindow.onresize(null), 3000);
            }

            const settingsProvider = new UrlSettingsProvider(config.settingsHostUrl);
            settingsProvider.getSettings(config.controlId)
                .then(settings => {
                    let idp;
                    let cacheService;
                    let dataSource: DataSource<any, any>;

                    if (settings.dataSourceSettings.auth && settings.dataSourceSettings.auth.type === 'Basic') {
                        // (window as any).userToken = btoa('kop' + ':' + '01E9z9eZ2+vuVcYZGifem1RrwhRSYY3cukp/S6/pK7w=');
                        idp = new BasicAuthenticationProvider();
                    }

                    if (settings.dataSourceSettings.cacheType === 'inMemory') {
                        cacheService = new InMemoryCacheService();
                    }

                    if (settings.dataSourceSettings.type === 'nav-api') {
                        if (settings.dataSourceSettings.resourceUrlFunc) {
                            const getResourceUrlFunction = new Function('urlData', settings.dataSourceSettings.resourceUrlFunc);
                            settings.dataSourceSettings.resourceUrl = getResourceUrlFunction(config.userInfo);
                        }

                        dataSource = new ODataDataSource<any, BasicUser>(settings.dataSourceSettings, idp, cacheService);
                    }

                    const initControlSettings: IControlInfo<ODataDataSource<any, BasicUser>, BasicUser> = {
                        id: config.controlId,
                        idp,
                        styleThemeProvider: new RemoteResourceThemeProvider('https://dev-meta-ui-grid.azureedge.net/1323'),
                        settingsProvider: settingsProvider,
                        dataSource: dataSource as any
                    };

                    const control = window.metaUI.initControl(initControlSettings, config.userInfo.userSecurityId);
                    const controlSpecificSubscriptions: Subscription[] = [];
                    control.events.onControlReady.subscribe({
                        next: item => {
                            controlSpecificSubscriptions.forEach(subscription => subscription.unsubscribe());

                            // Place to subscribe to the events
                            const instance: Grid = item.instance;
                            if (settings.dataSourceSettings.type === 'nav-api' || settings.dataSourceSettings.type === 'nav-webservice') {
                                instance.actions.forEach(action => {
                                    action.onExecute.subscribe(res => {
                                        //// TODO: implement event invoking
                                    });
                                });
                            }

                            // Row events
                            if (!(instance.events.row.onSelect instanceof FakeSubject)
                                && (instance.events.row.onSelect as any).subscribe) {
                                const onRowSelectSubscription = instance.events.row.onSelect.subscribe(row => {
                                    (window as any).Microsoft.Dynamics.NAV.InvokeExtensibilityMethod('OnRowSelected', [
                                        row.originalValue
                                    ]);
                                    console.log("Row onSelect event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowSelectSubscription);
                            }

                            if (!(instance.events.row.onClick instanceof FakeSubject)
                                && (instance.events.row.onClick as any).subscribe) {
                                const onRowClickSubscription = instance.events.row.onClick.subscribe(row => {
                                    console.log("Row onClick event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowClickSubscription);
                            }

                            if (!(instance.events.row.onRightClick instanceof FakeSubject)
                                && (instance.events.row.onRightClick as any).subscribe) {
                                const onRowRightClickSubscription = instance.events.row.onRightClick.subscribe(row => {
                                    console.log("Row onRightClick event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowRightClickSubscription);
                            }

                            if (!(instance.events.row.onDoubleClick instanceof FakeSubject)
                                && (instance.events.row.onDoubleClick as any).subscribe) {
                                const onRowDoubleClickSubscription = instance.events.row.onDoubleClick.subscribe(row => {
                                    console.log("Row onDoubleClick event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowDoubleClickSubscription);
                            }

                            if (!(instance.events.row.onDragStart instanceof FakeSubject)
                                && (instance.events.row.onDragStart as any).subscribe) {
                                const onRowDragStartSubscription = instance.events.row.onDragStart.subscribe(row => {
                                    console.log("Row onDragStart event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowDragStartSubscription);
                            }

                            if (!(instance.events.row.onDragEnd instanceof FakeSubject)
                                && (instance.events.row.onDragEnd as any).subscribe) {
                                const onRowDragEndSubscription = instance.events.row.onDragEnd.subscribe(row => {
                                    console.log("Row onDragEnd event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowDragEndSubscription);
                            }

                            if (!(instance.events.row.onHoverStart instanceof FakeSubject)
                                && (instance.events.row.onHoverStart as any).subscribe) {
                                const onRowHoverStartSubscription = instance.events.row.onHoverStart.subscribe(row => {
                                    console.log("Row onHoverStart event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowHoverStartSubscription);
                            }

                            if (!(instance.events.row.onHoverEnd instanceof FakeSubject)
                                && (instance.events.row.onHoverEnd as any).subscribe) {
                                const onRowHoverEndSubscription = instance.events.row.onHoverEnd.subscribe(row => {
                                    console.log("Row onHoverEnd event raised:");
                                    console.log(row);
                                })
                                controlSpecificSubscriptions.push(onRowHoverEndSubscription);
                            }

                            // Cell events
                            if (!(instance.events.cell.onClick instanceof FakeSubject)
                                && (instance.events.cell.onClick as any).subscribe) {
                                const onCellClickSubscription = instance.events.cell.onClick.subscribe(cell => {
                                    console.log("Cell onClick event raised:");
                                    console.log(cell);
                                })
                                controlSpecificSubscriptions.push(onCellClickSubscription);
                            }

                            if (!(instance.events.cell.onDoubleClick instanceof FakeSubject)
                                && (instance.events.cell.onDoubleClick as any).subscribe) {
                                const onCellDoubleClickSubscription = instance.events.cell.onDoubleClick.subscribe(cell => {
                                    console.log("Cell onDoubleClick event raised:");
                                    console.log(cell);
                                })
                                controlSpecificSubscriptions.push(onCellDoubleClickSubscription);
                            }

                            if (!(instance.events.cell.onRightClick instanceof FakeSubject)
                                && (instance.events.cell.onRightClick as any).subscribe) {
                                const onCellRightClickSubscription = instance.events.cell.onRightClick.subscribe(cell => {
                                    console.log("Cell onRightClick event raised:");
                                    console.log(cell);
                                })
                                controlSpecificSubscriptions.push(onCellRightClickSubscription);
                            }

                            // if (!(instance.events.cell.onDragStart instanceof FakeSubject)
                            //     && (instance.events.cell.onDragStart as any).subscribe) {
                            //     const onCellDragStartSubscription = instance.events.cell.onDragStart.subscribe(cell => {
                            //         console.log("Cell onDragStart event raised:");
                            //         console.log(cell);
                            //     })
                            //     controlSpecificSubscriptions.push(onCellDragStartSubscription);
                            // }

                            // if (!(instance.events.cell.onDragEnd instanceof FakeSubject)
                            //     && (instance.events.cell.onDragEnd as any).subscribe) {
                            //     const onCellDragEndSubscription = instance.events.cell.onDragEnd.subscribe(cell => {
                            //         console.log("Cell onDragEnd event raised:");
                            //         console.log(cell);
                            //     })
                            //     controlSpecificSubscriptions.push(onCellDragEndSubscription);
                            // }

                            if (!(instance.events.cell.onHoverStart instanceof FakeSubject)
                                && (instance.events.cell.onHoverStart as any).subscribe) {
                                const onCellHoverSubscription = instance.events.cell.onHoverStart.subscribe(cell => {
                                    console.log("Cell onHoverStart event raised:");
                                    console.log(cell);
                                })
                                controlSpecificSubscriptions.push(onCellHoverSubscription);
                            }

                            if (!(instance.events.cell.onHoverEnd instanceof FakeSubject)
                                && (instance.events.cell.onHoverEnd as any).subscribe) {
                                const onCellHoverSubscription = instance.events.cell.onHoverEnd.subscribe(cell => {
                                    console.log("Cell onHoverEnd event raised:");
                                    console.log(cell);
                                })
                                controlSpecificSubscriptions.push(onCellHoverSubscription);
                            }

                            // Column events
                            if (!(instance.events.column.onClick instanceof FakeSubject)
                                && (instance.events.column.onClick as any).subscribe) {
                                const onColumnClickSubscription = instance.events.column.onClick.subscribe(column => {
                                    console.log("Column onClick event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnClickSubscription);
                            }

                            if (!(instance.events.column.onDoubleClick instanceof FakeSubject)
                                && (instance.events.column.onDoubleClick as any).subscribe) {
                                const onColumnDoubleClickSubscription = instance.events.column.onDoubleClick.subscribe(column => {
                                    console.log("Column onDoubleClick event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnDoubleClickSubscription);
                            }

                            if (!(instance.events.column.onRightClick instanceof FakeSubject)
                                && (instance.events.column.onRightClick as any).subscribe) {
                                const onColumnRightClickSubscription = instance.events.column.onRightClick.subscribe(column => {
                                    console.log("Column onRightClick event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnRightClickSubscription);
                            }

                            if (!(instance.events.column.onDragStart instanceof FakeSubject)
                                && (instance.events.column.onDragStart as any).subscribe) {
                                const onColumnDragStartSubscription = instance.events.column.onDragStart.subscribe(column => {
                                    console.log("Column onDragStart event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnDragStartSubscription);
                            }

                            if (!(instance.events.column.onDragEnd instanceof FakeSubject)
                                && (instance.events.column.onDragEnd as any).subscribe) {
                                const onColumnDragEndSubscription = instance.events.column.onDragEnd.subscribe(column => {
                                    console.log("Column onDragEnd event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnDragEndSubscription);
                            }

                            if (!(instance.events.column.onResize instanceof FakeSubject)
                                && (instance.events.column.onResize as any).subscribe) {
                                const onColumnResizeSubscription = instance.events.column.onResize.subscribe(column => {
                                    console.log("Column onResize event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnResizeSubscription);
                            }

                            if (!(instance.events.column.onHoverStart instanceof FakeSubject)
                                && (instance.events.column.onHoverStart as any).subscribe) {
                                const onColumnHoverStartSubscription = instance.events.column.onHoverStart.subscribe(column => {
                                    console.log("Column onHoverStart event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnHoverStartSubscription);
                            }

                            if (!(instance.events.column.onHoverEnd instanceof FakeSubject)
                                && (instance.events.column.onHoverEnd as any).subscribe) {
                                const onColumnHoverEndSubscription = instance.events.column.onHoverEnd.subscribe(column => {
                                    console.log("Column onHoverEnd event raised:");
                                    console.log(column);
                                })
                                controlSpecificSubscriptions.push(onColumnHoverEndSubscription);
                            }

                            // Filters events
                            const onClearFiltersSubscription = dataSource.onClearFilters.subscribe(() => {
                                (window as any).Microsoft.Dynamics.NAV.InvokeExtensibilityMethod("OnClearFilters", []);
                            });
                            controlSpecificSubscriptions.push(onClearFiltersSubscription);

                            const onFiltersEmitSubscription = dataSource.onFiltersEmit.subscribe(filters => {
                                (window as any).Microsoft.Dynamics.NAV.InvokeExtensibilityMethod("OnFiltersEmit", [filters]);
                            });
                            controlSpecificSubscriptions.push(onFiltersEmitSubscription);

                            const parentWindows: Window[] = [];
                            let currentWindow: Window = window;
                            while (currentWindow.parent && currentWindow.parent !== currentWindow) {
                                parentWindows.push(currentWindow);
                                currentWindow = currentWindow.parent;
                            }

                            parentWindows.forEach(window => {
                                window.document.addEventListener('click', () => {
                                    Array.from(window.document.documentElement.getElementsByTagName('iframe'))
                                        .forEach(element => {
                                            if (element.contentDocument && element.contentDocument.body) {
                                                element.contentDocument.body.click();
                                            }
                                        });
                                });
                                window.document.addEventListener('keydown', (event) => {
                                    Array.from(window.document.documentElement.getElementsByTagName('iframe'))
                                        .forEach(element => {
                                            if (element.contentDocument && element.contentDocument.body) {
                                                element.contentDocument.body.dispatchEvent(event);
                                            }
                                        });
                                });
                            });

                            function resizeControlIframe(iframe: HTMLIFrameElement) {
                                let controlHeight = iframe.ownerDocument.documentElement.offsetHeight - iframe.getBoundingClientRect().top - 4;
                                iframe.style.minHeight = controlHeight + 'px';
                                iframe.style.maxHeight = controlHeight + 'px';
                                iframe.style.height = controlHeight + 'px';
                            }

                            if (parentWindows.length > 0) {
                                rootParentWindow = parentWindows[parentWindows.length - 1];
                                rootParentWindow.onresize = () => {
                                    rootParentWindow.document.documentElement.querySelectorAll('.control-addin-form')
                                        .forEach(element => {
                                            element.scrollTop = 0;
                                        })
                                    Array.from(rootParentWindow.document.documentElement.getElementsByTagName('iframe'))
                                        .forEach(element => {
                                            resizeControlIframe(element);
                                        });
                                }
                                rootParentWindow.onresize(null);
                            }

                            (window as any).Microsoft.Dynamics.NAV.InvokeExtensibilityMethod("OnControlReady", []);
                        }
                    });
                });
        }
    });
}