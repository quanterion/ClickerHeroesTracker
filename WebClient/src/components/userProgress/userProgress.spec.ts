import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { ActivatedRoute, Params } from "@angular/router";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import Decimal from "decimal.js";

import { UserProgressComponent } from "./userProgress";
import { UserService, IProgressData } from "../../services/userService/userService";
import { ChartDataSets, ChartPoint, ChartOptions } from "chart.js";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";

describe("UserProgressComponent", () => {
    let component: UserProgressComponent;
    let fixture: ComponentFixture<UserProgressComponent>;
    let routeParams: BehaviorSubject<Params>;

    let progress: IProgressData = {
        soulsSpentData: createData(0),
        titanDamageData: createData(1),
        heroSoulsSacrificedData: createData(2),
        totalAncientSoulsData: createData(3),
        transcendentPowerData: createData(4),
        rubiesData: createData(5),
        highestZoneThisTranscensionData: createData(6),
        highestZoneLifetimeData: createData(7),
        ascensionsThisTranscensionData: createData(8),
        ascensionsLifetimeData: createData(9),
        outsiderLevelData: {
            outsider0: createData(10),
            outsider1: createData(11),
            outsider2: createData(12),
        },
        ancientLevelData: {
            ancient0: createData(13),
            ancient1: createData(14),
            ancient2: createData(15),
        },
    };

    let expectedChartOrder = [
        { title: "Souls Spent", isLogarithmic: true, data: progress.soulsSpentData },
        { title: "Titan Damage", isLogarithmic: false, data: progress.titanDamageData },
        { title: "Hero Souls Sacrificed", isLogarithmic: true, data: progress.heroSoulsSacrificedData },
        { title: "Total Ancient Souls", isLogarithmic: false, data: progress.totalAncientSoulsData },
        { title: "Transcendent Power", isLogarithmic: true, data: progress.transcendentPowerData },
        { title: "Rubies", isLogarithmic: false, data: progress.rubiesData },
        { title: "Highest Zone This Transcension", isLogarithmic: true, data: progress.highestZoneThisTranscensionData },
        { title: "Highest Zone Lifetime", isLogarithmic: false, data: progress.highestZoneLifetimeData },
        { title: "Ascensions This Transcension", isLogarithmic: true, data: progress.ascensionsThisTranscensionData },
        { title: "Ascensions Lifetime", isLogarithmic: false, data: progress.ascensionsLifetimeData },
        { title: "Outsider0", isLogarithmic: true, data: progress.outsiderLevelData.outsider0 },
        { title: "Outsider1", isLogarithmic: false, data: progress.outsiderLevelData.outsider1 },
        { title: "Outsider2", isLogarithmic: true, data: progress.outsiderLevelData.outsider2 },
        { title: "Ancient0", isLogarithmic: false, data: progress.ancientLevelData.ancient0 },
        { title: "Ancient1", isLogarithmic: true, data: progress.ancientLevelData.ancient1 },
        { title: "Ancient2", isLogarithmic: false, data: progress.ancientLevelData.ancient2 },
    ];

    let settings: IUserSettings = {
        areUploadsPublic: true,
        playStyle: "hybrid",
        useScientificNotation: true,
        scientificNotationThreshold: 1000000,
        useEffectiveLevelForSuggestions: false,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 2,
        theme: "light",
    };

    let settingsSubject = new BehaviorSubject(settings);

    beforeEach(async(() => {
        let userService = {
            getProgress(): Promise<IProgressData> {
                return Promise.resolve(progress);
            },
        };

        let settingsService = { settings: () => settingsSubject };

        routeParams = new BehaviorSubject({ userName: "someUserName" });
        let route = { params: routeParams };
        TestBed.configureTestingModule(
            {
                declarations: [UserProgressComponent],
                providers: [
                    { provide: ActivatedRoute, useValue: route },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(UserProgressComponent);
                component = fixture.componentInstance;
            });
    }));

    describe("Range Selector", () => {
        it("should display", () => {
            fixture.detectChanges();

            expect(component.currentDateRange).toEqual("1w");

            let dateRangeSelector = fixture.debugElement.query(By.css(".btn-group"));
            expect(dateRangeSelector).not.toBeNull();

            let dateRanges = dateRangeSelector.queryAll(By.css("button"));
            expect(dateRanges.length).toEqual(component.dateRanges.length);
            for (let i = 0; i < dateRanges.length; i++) {
                expect(dateRanges[i].nativeElement.textContent.trim()).toEqual(component.dateRanges[i]);
            }

            let disabledDateRanges = dateRanges.filter(dateRange => dateRange.classes.disabled);
            expect(disabledDateRanges.length).toEqual(1);
            expect(disabledDateRanges[0].nativeElement.textContent.trim()).toEqual(component.currentDateRange);
        });

        it("should change the current date range when clicked", () => {
            fixture.detectChanges();

            expect(component.currentDateRange).toEqual("1w");

            let dateRangeSelector = fixture.debugElement.query(By.css(".btn-group"));
            expect(dateRangeSelector).not.toBeNull();

            let dateRanges = dateRangeSelector.queryAll(By.css("button"));
            expect(dateRanges.length).toEqual(component.dateRanges.length);

            dateRanges[dateRanges.length - 1].nativeElement.click();

            fixture.detectChanges();

            expect(component.currentDateRange).toEqual(component.dateRanges[dateRanges.length - 1]);

            let disabledDateRanges = dateRanges.filter(dateRange => dateRange.classes.disabled);
            expect(disabledDateRanges.length).toEqual(1);
            expect(disabledDateRanges[0].nativeElement.textContent.trim()).toEqual(component.currentDateRange);
        });

        it("should request the proper date range", () => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.callThrough();

            fixture.detectChanges();

            let now = Date.now();
            spyOn(Date, "now").and.returnValue(now);

            let expectedEnd = new Date(now);
            let expectedStarts: { [dateRange: string]: Date } = {
                "1d": new Date(new Date(now).setDate(expectedEnd.getDate() - 1)),
                "3d": new Date(new Date(now).setDate(expectedEnd.getDate() - 3)),
                "1w": new Date(new Date(now).setDate(expectedEnd.getDate() - 7)),
                "1m": new Date(new Date(now).setMonth(expectedEnd.getMonth() - 1)),
                "3m": new Date(new Date(now).setMonth(expectedEnd.getMonth() - 3)),
                "1y": new Date(new Date(now).setFullYear(expectedEnd.getFullYear() - 1)),
            };

            expect(component.dateRanges.length).toEqual(Object.keys(expectedStarts).length);

            for (let i = 0; i < component.dateRanges.length; i++) {
                let dateRange = component.dateRanges[i];
                (userService.getProgress as jasmine.Spy).calls.reset();

                component.currentDateRange = dateRange;

                expect(userService.getProgress).toHaveBeenCalledWith("someUserName", expectedStarts[dateRange], expectedEnd);
            }
        });
    });

    describe("Charts", () => {
        it("should display", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();

                    let charts = fixture.debugElement.queryAll(By.css("canvas"));
                    expect(charts.length).toEqual(16);

                    for (let i = 0; i < charts.length; i++) {
                        let chart = charts[i];

                        expect(chart).not.toBeNull();
                        expect(chart.attributes.baseChart).toBeDefined();
                        expect(chart.attributes.height).toEqual("235");
                        expect(chart.properties.chartType).toEqual("line");

                        let colors = chart.properties.colors;
                        expect(colors).toBeTruthy();

                        let options: ChartOptions = chart.properties.options;
                        expect(options).toBeTruthy();
                        expect(options.title.text).toEqual(expectedChartOrder[i].title);

                        let datasets: ChartDataSets[] = chart.properties.datasets;
                        expect(datasets).toBeTruthy();
                        expect(datasets.length).toEqual(1);

                        let data = datasets[0].data as ChartPoint[];
                        let expectedData = expectedChartOrder[i].data;
                        let isLogarithmic = expectedChartOrder[i].isLogarithmic;
                        let dataKeys = Object.keys(expectedData);
                        expect(data.length).toEqual(dataKeys.length);
                        for (let j = 0; j < data.length; j++) {
                            let expectedDate = new Date(dataKeys[j]);
                            expect(data[j].x).toEqual(expectedDate.getTime());
                            expect((options.tooltips.callbacks.title as Function)([{ xLabel: dataKeys[j] }])).toEqual(expectedDate.toLocaleString());

                            // When logarithmic, the value we plot is actually the log of the value to fake log scale
                            let rawExpectedValue = expectedData[dataKeys[j]];
                            let expectedValue = isLogarithmic
                                ? new Decimal(rawExpectedValue).log().toNumber()
                                : Number(rawExpectedValue);
                            expect(data[j].y).toEqual(expectedValue);

                            let expectedLabel = isLogarithmic
                                ? Decimal.pow(10, expectedValue).toExponential(3)
                                : Number(expectedValue).toExponential(3);
                            expect((options.tooltips.callbacks.label as Function)({ yLabel: expectedValue })).toEqual(expectedLabel);
                            expect(options.scales.yAxes[0].ticks.callback(expectedValue, null, null)).toEqual(expectedLabel);
                        }

                        // Linear even when logarithmic since we're manually managing log scale
                        expect(options.scales.yAxes[0].type).toEqual("linear");
                    }
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when userService.getProgress fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).toBeNull();

                    let charts = fixture.debugElement.queryAll(By.css("canvas"));
                    expect(charts.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a warning when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({}));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let error = fixture.debugElement.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = fixture.debugElement.query(By.css(".text-warning"));
                    expect(warning).not.toBeNull();

                    let charts = fixture.debugElement.queryAll(By.css("canvas"));
                    expect(charts.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });
    });

    function createData(index: number): { [date: string]: string } {
        // Mix it up to let some charts be logarithmic
        let prefix = index % 2
            ? index.toString()
            : "1e10" + index;
        return {
            "2017-01-01T00:00:00Z": prefix + "1",
            "2017-01-02T00:00:00Z": prefix + "2",
            "2017-01-03T00:00:00Z": prefix + "3",
            "2017-01-04T00:00:00Z": prefix + "4",
            "2017-01-05T00:00:00Z": prefix + "5",
        };
    }
});
