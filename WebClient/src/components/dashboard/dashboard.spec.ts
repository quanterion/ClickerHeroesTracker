import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import Decimal from "decimal.js";
import { ChartDataSets, ChartOptions, ChartPoint } from "chart.js";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { DashboardComponent } from "./dashboard";
import { UserService, IProgressData, IFollowsData } from "../../services/userService/userService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";

describe("DashboardComponent", () => {
    let component: DashboardComponent;
    let fixture: ComponentFixture<DashboardComponent>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    let userInfo = new BehaviorSubject(loggedInUser);

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

    let progress: IProgressData = {
        soulsSpentData: {
            "2017-01-01T00:00:00Z": "0",
            "2017-01-02T00:00:00Z": "1",
            "2017-01-03T00:00:00Z": "2",
            "2017-01-04T00:00:00Z": "3",
            "2017-01-05T00:00:00Z": "4",
        },
        titanDamageData: undefined,
        heroSoulsSacrificedData: undefined,
        totalAncientSoulsData: undefined,
        transcendentPowerData: undefined,
        rubiesData: undefined,
        highestZoneThisTranscensionData: undefined,
        highestZoneLifetimeData: undefined,
        ascensionsThisTranscensionData: undefined,
        ascensionsLifetimeData: undefined,
        ancientLevelData: undefined,
        outsiderLevelData: undefined,
    };

    let followsData: IFollowsData = {
        follows: [
            "followedUser1",
            "followedUser2",
            "followedUser3",
        ],
    };

    beforeEach(done => {
        let authenticationService = { userInfo: () => userInfo };
        let userService = {
            getProgress: () => Promise.resolve(progress),
            getFollows: () => Promise.resolve(followsData),
        };
        let settingsService = { settings: () => settingsSubject };

        TestBed.configureTestingModule(
            {
                declarations: [DashboardComponent],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: UserService, useValue: userService },
                    { provide: SettingsService, useValue: settingsService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(DashboardComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    describe("Upload Table", () => {
        it("should display without pagination", () => {
            fixture.detectChanges();

            let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
            expect(containers.length).toEqual(3);

            let uploadsContainer = containers[0];

            let uploadsTable = uploadsContainer.query(By.css("uploadsTable"));
            expect(uploadsTable).not.toBeNull();
            expect(uploadsTable.properties.count).toEqual(10);
            expect(uploadsTable.properties.paginate).toBeFalsy();
        });
    });

    describe("Progress Summary", () => {
        it("should display a chart with linear scale", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let progressContainer = containers[1];

                    let error = progressContainer.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = progressContainer.query(By.css(".text-warning"));
                    expect(warning).toBeNull();

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).not.toBeNull();
                    expect(chart.attributes.baseChart).toBeDefined();
                    expect(chart.attributes.height).toEqual("235");
                    expect(chart.properties.chartType).toEqual("line");

                    let datasets: ChartDataSets[] = chart.properties.datasets;
                    expect(datasets).toBeTruthy();
                    expect(datasets.length).toEqual(1);

                    let data = datasets[0].data as ChartPoint[];
                    let dataKeys = Object.keys(progress.soulsSpentData);
                    expect(data.length).toEqual(dataKeys.length);
                    for (let i = 0; i < data.length; i++) {
                        expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());
                        expect(data[i].y).toEqual(Number(progress.soulsSpentData[dataKeys[i]]));
                    }

                    let colors = chart.properties.colors;
                    expect(colors).toBeTruthy();

                    let options: ChartOptions = chart.properties.options;
                    expect(options).toBeTruthy();
                    expect(options.title.text).toEqual("Souls Spent");
                    expect(options.scales.yAxes[0].type).toEqual("linear");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should display a chart with logarithmic scale", done => {
            // Using values greater than normal numbers can handle
            let soulsSpentData: { [date: string]: string } = {
                "2017-01-01T00:00:00Z": "1e1000",
                "2017-01-02T00:00:00Z": "1e1001",
                "2017-01-03T00:00:00Z": "1e1002",
                "2017-01-04T00:00:00Z": "1e1003",
                "2017-01-05T00:00:00Z": "1e1004",
            };
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData }));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let progressContainer = containers[1];

                    let error = progressContainer.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = progressContainer.query(By.css(".text-warning"));
                    expect(warning).toBeNull();

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).not.toBeNull();
                    expect(chart.attributes.baseChart).toBeDefined();
                    expect(chart.attributes.height).toEqual("235");
                    expect(chart.properties.chartType).toEqual("line");

                    let datasets: ChartDataSets[] = chart.properties.datasets;
                    expect(datasets).toBeTruthy();
                    expect(datasets.length).toEqual(1);

                    let data = datasets[0].data as ChartPoint[];
                    let dataKeys = Object.keys(soulsSpentData);
                    expect(data.length).toEqual(dataKeys.length);
                    for (let i = 0; i < data.length; i++) {
                        expect(data[i].x).toEqual(new Date(dataKeys[i]).getTime());

                        // The value we plot is actually the log of the value to fake log scale
                        expect(data[i].y).toEqual(new Decimal(soulsSpentData[dataKeys[i]]).log().toNumber());
                    }

                    let colors = chart.properties.colors;
                    expect(colors).toBeTruthy();

                    let options: ChartOptions = chart.properties.options;
                    expect(options).toBeTruthy();
                    expect(options.title.text).toEqual("Souls Spent");

                    // Linear since we're manually managing log scale
                    expect(options.scales.yAxes[0].type).toEqual("linear");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when the user is not logged in", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "userInfo").and.returnValue(new BehaviorSubject(notLoggedInUser));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let progressContainer = containers[1];

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = progressContainer.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();

                    let warning = progressContainer.query(By.css(".text-warning"));
                    expect(warning).toBeNull();
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

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let progressContainer = containers[1];

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = progressContainer.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();

                    let warning = progressContainer.query(By.css(".text-warning"));
                    expect(warning).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a warning when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getProgress").and.returnValue(Promise.resolve({ soulsSpentData: {} }));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let progressContainer = containers[1];

                    let chart = progressContainer.query(By.css("canvas"));
                    expect(chart).toBeNull();

                    let error = progressContainer.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let warning = progressContainer.query(By.css(".text-warning"));
                    expect(warning).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Follows", () => {
        it("should display the table", done => {
            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let followsContainer = containers[2];

                    let error = followsContainer.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let noData = followsContainer.query(By.css("p:not(.text-danger)"));
                    expect(noData).toBeNull();

                    let table = followsContainer.query(By.css("table"));
                    expect(table).not.toBeNull();

                    let rows = table.query(By.css("tbody")).children;
                    expect(rows.length).toEqual(followsData.follows.length);

                    for (let i = 0; i < rows.length; i++) {
                        let expectedFollow = followsData.follows[i];

                        let cells = rows[i].children;
                        expect(cells.length).toEqual(2);

                        let followCell = cells[0];
                        expect(followCell.nativeElement.textContent.trim()).toEqual(expectedFollow);

                        let compareCell = cells[1];
                        let link = compareCell.query(By.css("a"));
                        expect(link.properties.routerLink).toEqual(`/users/${loggedInUser.username}/compare/${expectedFollow}`);
                        expect(link.nativeElement.textContent.trim()).toEqual("Compare");
                    }
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when userService.getFollows fails", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.reject("someReason"));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let followsContainer = containers[2];

                    let table = followsContainer.query(By.css("table"));
                    expect(table).toBeNull();

                    let noData = followsContainer.query(By.css("p:not(.text-danger)"));
                    expect(noData).toBeNull();

                    let error = followsContainer.query(By.css(".text-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show a message when there is no data", done => {
            let userService = TestBed.get(UserService);
            spyOn(userService, "getFollows").and.returnValue(Promise.resolve({}));

            fixture.detectChanges();
            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
                    expect(containers.length).toEqual(3);

                    let followsContainer = containers[2];

                    let table = followsContainer.query(By.css("table"));
                    expect(table).toBeNull();

                    let error = followsContainer.query(By.css(".text-danger"));
                    expect(error).toBeNull();

                    let noData = followsContainer.query(By.css("p:not(.text-danger)"));
                    expect(noData).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });
});
