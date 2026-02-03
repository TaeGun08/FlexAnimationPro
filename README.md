# FlexAnimation Pro (플렉스 애니메이션 프로)

FlexAnimation Pro는 Unity 프로젝트를 위한 강력하고 유연한 모듈형 애니메이션 시스템입니다.  
복잡한 코딩 없이 인스펙터(Inspector)에서 블록을 쌓듯이 애니메이션을 구성하거나, 가벼운 자체 트위닝 엔진을 통해 효율적인 애니메이션 제어가 가능합니다.

## ✨ 주요 특징

*   **인스펙터 중심 워크플로우 (Inspector-First):** 코드를 한 줄도 작성하지 않고도 `Move`, `Rotate`, `Scale` 등의 모듈을 조합하여 복잡한 시퀀스 애니메이션을 만들 수 있습니다.
*   **독립적인 트위닝 엔진:** 외부 라이브러리(DOTween 등) 없이도 완벽하게 동작하는 자체 코루틴 기반 트위닝 시스템(`FlexTween`)이 내장되어 있습니다. (DOTween이 설치된 경우 연동 가능)
*   **강력한 텍스트 애니메이션:** TextMeshPro와 연동되어 글자 단위의 화려한 등장 효과(Flip, Spin, Glitch 등)와 스크램블 효과를 제공합니다.
*   **다양한 애니메이션 모듈:** 기본 변환부터 오디오, 이벤트, UI 전용 제어까지 포괄적인 모듈을 제공합니다.
*   **풍부한 이징(Easing) 함수:** Linear, Sine, Quad, Cubic, Elastic, Bounce 등 30가지 이상의 이징 함수를 기본 제공하여 자연스러운 움직임을 구현합니다.
*   **확장성:** `AnimationModule` 클래스를 상속받아 나만의 커스텀 애니메이션 모듈을 쉽게 추가할 수 있습니다.

## 📦 설치 방법

이 패키지 폴더를 프로젝트의 `Assets` 폴더 내 원하는 위치에 복사하세요.

> **참고:** 어셈블리 정의 파일(`FlexAnimation.Runtime.asmdef`)이 포함되어 있어, 별도의 어셈블리로 관리되므로 컴파일 시간을 절약할 수 있습니다.

## 🚀 빠른 시작 (Quick Start)

### 1. 컴포넌트 추가
애니메이션을 적용할 GameObject에 `FlexAnimation` 컴포넌트를 추가합니다.

### 2. 모듈 추가
인스펙터의 **Modules** 리스트에서 `+` 버튼을 눌러 원하는 애니메이션 모듈을 추가합니다.
*   예: `MoveModule` (이동), `ScaleModule` (크기 조절)

### 3. 애니메이션 설정
각 모듈의 속성을 설정합니다.
*   **Duration:** 애니메이션 지속 시간 (초)
*   **Delay:** 시작 전 대기 시간
*   **Ease:** 움직임의 가속/감속 그래프 (예: `OutBack`, `InOutQuad`)
*   **Link Type:**
    *   `Append`: 이전 애니메이션이 끝난 후 실행 (순차 실행)
    *   `Join`: 이전 애니메이션과 동시에 실행

### 4. 실행
*   **Play On Enable** 옵션이 체크되어 있으면, 오브젝트가 활성화될 때 자동으로 재생됩니다.
*   스크립트에서 제어하려면 아래 "스크립트 제어" 항목을 참고하세요.

## 💻 스크립트 제어 (API)

```csharp
using FlexAnimation;

public class MyScript : MonoBehaviour
{
    public FlexAnimation flexAnim;

    void Start()
    {
        // 애니메이션 재생
        flexAnim.PlayAll();
    }

    void Stop()
    {
        // 중지 및 초기 상태로 리셋
        flexAnim.StopAndReset();
    }
    
    // 이벤트 등록 예시
    void SetupEvents()
    {
        flexAnim.OnPlay.AddListener(() => Debug.Log("시작됨"));
        flexAnim.OnComplete.AddListener(() => Debug.Log("완료됨"));
    }
}
```

## 🧩 모듈 상세 설명

### 기본 변환 (Transform)
| 모듈 이름 | 설명 | 주요 기능 |
| :--- | :--- | :--- |
| **MoveModule** | 오브젝트의 위치를 이동합니다. | World/Local 좌표, AnchoredPosition(UI), 랜덤 범위 설정 |
| **RotateModule** | 오브젝트를 회전시킵니다. | Euler Angles, 상대적 회전 |
| **ScaleModule** | 오브젝트의 크기를 변경합니다. | 절대값/상대값 크기 변경 |

### 그래픽 및 이펙트 (Graphics & Effects)
| 모듈 이름 | 설명 | 주요 기능 |
| :--- | :--- | :--- |
| **FadeModule** | 투명도(Alpha)를 조절합니다. | CanvasGroup, Image, SpriteRenderer 자동 감지 |
| **ColorModule** | 색상(Color)을 변경합니다. | Graphic(UI), SpriteRenderer, Light 색상 지원 |
| **MaterialModule** | 매테리얼 속성을 변경합니다. | Shader 프로퍼티(Float, Color, Vector) 제어 |
| **PunchModule** | 일시적인 강한 충격 효과를 줍니다. | 위치/회전/크기 펀치, 진동수(Vibrato), 탄성 조절 |
| **ShakeModule** | 지속적인 흔들림 효과를 줍니다. | 강도(Strength), 무작위 흔들림 |

### UI 및 텍스트 (UI & Text)
| 모듈 이름 | 설명 | 주요 기능 |
| :--- | :--- | :--- |
| **UIModule** | UI 요소(RectTransform)를 제어합니다. | AnchoredPosition, SizeDelta(너비/높이) 정밀 제어 |
| **TextModule** | **(강력함)** TextMeshPro 텍스트 애니메이션 | **Transition:** Flip, Spin, Zoom, Vortex, Slide<br>**Effects:** Glitch, Wave, Shake<br>**Scramble:** Matrix(Random), Numeric(숫자 카운팅), Binary |

### 유틸리티 (Utility)
| 모듈 이름 | 설명 | 주요 기능 |
| :--- | :--- | :--- |
| **AudioModule** | 오디오 효과음을 재생합니다. | AudioClip 재생, 볼륨 조절, 지연 실행 |
| **EventModule** | 이벤트를 트리거합니다. | UnityEvent 호출 (함수 실행), 시퀀스 중간에 로직 삽입 |

---

작성일: 2026년 2월 3일
