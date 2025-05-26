[시연 영상 URL](https://drive.google.com/drive/folders/1NvVBL-YVmYNje2AlgfEuIPu6MtchEOlj)

***

## (1)  핵심 기능

> -  VR 환경 지원
> -  별자리 시뮬레이션
> -  별 궤도 시뮬레이션

***

## (2)  요구사항 및 시스템 구성

| 기능                | 세부사항                                                           | 관련 스크립트                                                                                                                             |
| :---------------- | -------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| - VR 환경에서 UI 활용   |                                                                | - /Script/UI 디렉토리 내의 모든 스크립트<br>- ToggleNearFarController.cs<br>- DisableGrabbingHandModel.cs<br>- PhysicalRig.cs<br>- TurnLight.cs |
| - 별자리 관측          | - 별 프리팹 설계 및 생성<br>- 별자리 선 연결<br>                              | - StarSpawner.cs                                                                                                                    |
| - 별 궤도 관측         |                                                                | - Trajectory.cs<br>- CelestialPathDrawer.cs                                                                                         |
| - 달 관측            | - 달 프리팹 설계 및 생성                                                | - Moon.cs<br>                                                                                                                       |
| - 관측자의 위치 및 시각 지정 | - 위도/경도 설정<br>- 연/월/일/시/분/초 설정<br>- 지구의 자전/공전 반영<br>- 맵의 낮밤 전환 | - StarPositionCalculator.cs<br>- TimeManager.cs<br>- DayNightCycle.cs<br>- SkyboxSunController.cs                                   |
| - 스크린샷 촬영 기능      |                                                                | - ScreenshotHandler.cs<br>- ShutterSoundPlayer.cs                                                                                   |

***

## (3)  어려웠던 점 & 문제점

### 1.  별자리 위치 계산

1.  변인이 너무 많아서 구현 난이도가 높을 것으로 예상 -> 관측 위치(위도/경도), 관측 시각(연/월/일/시/분/초), 지구의 공전과 자전.
2.  천구 모양의 지구를 실제로 설계하여 관측 위치를 실제로 달리하는 방식? -> 관측자가 남반구에 위치할 경우 유니티 Gravity 세팅의 영향을 받음. 따라서 물리 법칙을 적절히 구현하여야 하는데, 별이나 달 등 다른 오브젝트와의 상호작용 통제 어려울 것으로 예상.
3.  별들의 상대적인 위치를 그때그때 계산하여 별자리들을 회전시키는 방식 채택.
4.  변인들을 최대한 독립적으로 모듈화하여 변인 간 간섭을 최소화하고 프로그램 가독성을 향상 -> \[ 여기에 '어떤 함수, 어떤 함수가 있다'하고 정리해보고 싶었는데 코드 구조 파악이 잘 안 되어서... 수민님께서 직접 판단하시고 넣을지 말지 편하게 결정하세요!! \]
5.  관측자가 아닌 별자리를 이동시키므로 시각적으로 흥미로울 뿐만 아니라, 모든 별들의 궤적을 모아서 관측할 수 있도록 하는 독창적인 기능을 연상하여 실제로 도입할 수 있었음.

\[ 이쯤에서 '위도 경도 변경.mp4' 보여주면 맥락상 매끄럽게 잘 마무리될 것 같습니다. ]


### 2. Direction Light 문제

1.  낮밤 전환을 위해 추가한 Directional Light가 의도치 않게 밤에도 맵의 일부 오브젝트를 비추는 현상 발생.
2.  태양이 지고 난 후 특정 시간대부터 Directional Light를 unabled시키는 방식은 해당 프레임에서 Scene이 갑작스럽게 어두워지므로 부자연스러움을 확인.
3.  unabled시키는 대신, Light가 맵에 영향을 미치지 않아야 하는 시간대(밤)에서만 Light 타입을 변경하는 방식을 통해 광량의 변화가 최대한 자연스럽게 연결함으로써 해결.


### 3.  최적화

1.  수많은 오브젝트들에 대해 한꺼번에 행렬 연산을 진행하다 보니 초당 프레임 수를 안정화하는 데 어려움을 겪음.
2.  해결 시 별자리를 이루지 않는 일반 별들 역시 씬에 추가함으로써 시각적인 만족도를 향상시킬 수 있을 것으로 예상.
3.  추후 다른 프로젝트 진행 시, 사용자와의 상호작용 여부에 따라 오브젝트들을 각각 어떻게 최적화를 진행할 수 있을지 고민해 보아야겠다고 생각하는 계기가 됨.

***

## (4)  결과물

플레이 영상 링크
https://drive.google.com/drive/folders/1NvVBL-YVmYNje2AlgfEuIPu6MtchEOlj

***

## (5)  팀원별 기여도

#### 노준서
| 내용           | 활용 기술, 작성한 스크립트 등         |
| :----------- | ------------------------- |
| - 궤적 표시 기능   | - Trajectory.cs 작성        |
| - 스크린샷 촬영 기능 | - ScreenshotHandler.cs 작성 |

#### 송용환
| 내용                | 활용 기술, 작성한 스크립트 등                                                                          |
| :---------------- | ------------------------------------------------------------------------------------------ |
| - 별 데이터 수집        | - dummyStars.json 추가                                                                       |
| - 별 분광형에 따른 색상 적용 | - StarSpawner.cs의 getColorFromSpectralTypes() 작성<br>- StarSpawner.cs의 EnhanceContrast() 작성 |
| - Star 매터리얼 제작    | - Shader Graph 활용                                                                          |
| - 낮밤 전환 기능        | - DayNightCycle.cs 작성<br>- TimeManager.cs에 Light System 연동                                 |

#### 이준성
| 내용                         | 활용 기술, 작성한 스크립트 등                                                                                                              |
| :------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| - 별자리 데이터 수집               | - simbad_results.json 추가<br>- constellation_lines.json 추가                                                                      |
| - 별자리 데이터 적용               | - StarSpawner.cs의 LoadStarsFromJson() 작성<br>- StarSpawner.cs의 CreateStar() 작성<br>- StarSpawner.cs의 DrawConstellationLines() 작성 |
| - UI 구현                    | - 주요 기능과 관련된, **모든** View 클래스와 Control 클래스 작성<br>(/Script/UI 디렉토리 내의 CelestialControlPanel, CelestialScenceController 등)       |
| - VR 기능 구현                 | - XRI interactionToolkit 활용                                                                                                    |
| - VR 기능 테스트                | - 시연 녹화하여 팀원들에게 개발 현황 공유                                                                                                       |
| - 최적화                      | - Occulsion 활용                                                                                                                 |
| - 맵 제작 (배경음악 선정, 필요 에셋 탐색) | - MusicManager.cs 작성<br>- MuscinZone.cs 작성                                                                                     |
| - 스크린샷 삭제 기능               | - ScreenshotHandler.cs의 OnDeleteButtonClicked() 작성                                                                             |
| - 낮밤 전환 기능                 | - SkyboxSunController.cs 작성                                                                                                    |

#### 조수민
| 내용                         | 활용 기술, 작성한 스크립트 등                                                                                                                   |
| :------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| - 별 분광형에 따른 색상 테이블 수집      | - 출처 : https://arxiv.org/pdf/2101.06254                                                                                             |
| - 별 데이터 적용                 | - StarSpawner.cs의 LoadStarsFromJson() 수정<br>- StarSpawner.cs의 CreateDummyStar() 작성<br>- StarSpawner.cs의 DrawConstellationLines() 수정 |
| - 관측자 날짜 선택 (연/월/일/시/분/초)  | - TimeManager.cs 작성                                                                                                                 |
| - 관측자 위치 선택 (위도/경도)        | - StarSpawner.cs의 CalculateStarPosition() 작성<br>                                                                                    |
| - 관측자 날짜 및 위치에 기반한 별 위치 계산 | - StarPositionCalculator.cs 작성                                                                                                      |
| - 관측자 날짜에 기반한 달 위상 계산      | - Moon.cs 작성                                                                                                                        |
| - Moon 매터리얼 제작             | - Shader Graph 활용                                                                                                                   |
| - 궤적 Duration 지정 기능 추가     | - CelestialPathDrawer.cs 작성        
