# 이미지 생성 제작 내역

사용 도구: 내장 image_gen (투명 배경, 로컬 기존 이미지 편집). Unity가 PNG 시트를 역할/프레임별로 잘라 기존 런타임 경로와 GUID에 연결했다.

공통 지시: 붉은 머리 소녀와 동료 모두 약3등신, 머리는 키의 약1/3, 짧고 둥근 몸, 오른쪽 옆모습, 기존 겨울 모피·가죽 복장과 역할별 장비, 픽셀 스타일, 투명 배경. 단순 세로 압축 대신 캐릭터를 다시 그리도록 요청했다.

## axe.png 최종 프롬프트
Edit target: this exact game sprite sheet. Redesign the same red-haired young female winter adventurer as a coherent THREE-HEADS-TALL chibi pixel-art character: head from hair crown to chin is exactly one third of standing body height (excluding axe), compact torso and short legs. Keep long vivid red hair, brown leather winter outfit, cream fur cuffs, brown boots, large steel axe, side profile facing RIGHT. Preserve all SIXTEEN poses and their exact order in an evenly spaced 4 columns x 4 rows atlas. Row1 four idle breathing poses; row2 four walking cycle poses; row3 axe combat windup, raised axe, downswing, followthrough; row4 collapse and death sequence ending lying flat. Exactly one complete isolated figure per cell, no crossing cell boundaries, generous transparent padding, boots baseline consistent across standing frames. Enlarge heads, shorten bodies, never just vertically squash. Crisp unified hand drawn pixel-art, limited warm earthy palette, clear eyes. Transparent background, no scenery, no shadows, no text, no grid lines. Square output. Every standing character MUST read as 3 heads tall.

## bow.png 최종 보정 프롬프트
Correct production defects in this 4x4 pixel-art red-haired three-head-tall chibi archer animation sheet. Keep girl, short-body proportions, all16 poses/order/clothing. Exactly one figure completely inside each equal cell with 8% transparent padding. NO flying or detached arrows anywhere: remove the long released arrow extending from row3 col3 toward col4. Row3 shows aiming/drawing/releasing/relaxing bow, but all bow/arrow details must remain inside own cell. Every complete figure must be isolated from neighboring cells. Maintain4idle/4walk/4bowaction/4collapse poses. Transparent background, no labels, shadows, gridlines. Square sheet.

## logging.png 최종 보정 프롬프트
Correct this 4-column 3-row pixel-art three-head-tall red-haired chibi logging sprite atlas. Preserve exactly all twelve right-facing poses, girl's proportions, clothes, red hair and axe. Important production fix: fit EVERY complete axe and character entirely inside its own equal grid cell with at least 10% transparent padding on all four cell edges. Especially top row fourth raised axe must NOT touch or be clipped by image top, and row2 fourth raised axe must stay below its cell top. Make all twelve figures 15% smaller within their cells without changing proportions. Uniform scale across poses. No text, grid, background, shadows. Transparent4:3sheet.

## companions.png 최종 보정 프롬프트
Edit FIRST atlas, KEEP all 8 roles/clothes/tools/ordering/right-facing views. Correct remaining too-tall anatomy: give EVERY companion a MUCH LARGER HEAD and MUCH SHORTER BODY matching SECOND girl. They currently look 4-5 heads tall; target very squat adorable THREE-HEAD-TALL chibi. Each head crown-to-chin must occupy at least 35% of the whole figure's height, with the compact fur-clothed torso and short boots occupying only remaining 65%. Enlarge face and cranium visibly by 50%, compress torso and legs dramatically; maintain adult identities. Hunter, helmet warrior woman, farmer woman, cook man top row; healer, aunt, uncle, child bottom row. Exactly4x2 equal cells, no cell crossings, entire figure and tools within cell, leave 8% clear transparent margins on all edges of every cell. Match red-haired girl's pixel-art style, not realistic body proportions. No shadows backgrounds labels lines. Transparent.

## 배치
- 게임 원본: C:/Development/6_IL/Assets/art/generated/characters/chibi20261018/
- 주인공: Assets/Resources/Sprites/Player/RedHairGirl/axe.png, bow.png, logging.png
- 동료: 기존 GeneratedStatic20261014/Companions 5종과 Sprites/Companions 3종
- 프레임 순서·이름·GUID 유지. 개별 동료 PNG는 이 폴더의 동료/에 포함.
