# AR-Flashcards-Pronunciation# AR-Flashcards-Pronunciation



**Aplicación educativa de Realidad Aumentada para practicar vocabulario del aula con pronunciación interactiva.**App AR educativa (Unity + Vuforia 9.8) para practicar vocabulario del aula.

Escanea tarjetas (Image Targets), presiona el **Virtual Button** y verás un **overlay** con el nombre en inglés, su **pronunciación (IPA)** y podrás **reproducir audio**. Incluye un **HUD siempre visible** para guiar el escaneo.

App AR para practicar vocabulario del aula. Escanea tarjetas (Image Targets), presiona el **Virtual Button** y verás un panel con el nombre en inglés, la pronunciación (IPA) y audio. Incluye un HUD que guía el escaneo.

## ✨ Funcionalidades

---- Detección de tarjetas con **Vuforia 9.8**.

- **Virtual Button**: muestra/oculta el panel de información.

## 📑 Tabla de Contenidos- **Audio por palabra**: arrastrable por tarjeta o fallback desde `Resources/tts`.

- **HUD** permanente: “Apunta a una tarjeta / ¡Detectado!”.

1. [Resumen del Proyecto](#-resumen-del-proyecto)- UI moderna con paleta **Glimmer**:  

2. [Características Principales](#-características-principales)  - Glimmer-1 `#A6586D`  

3. [Paleta de Colores y Tipografía](#-paleta-de-colores-y-tipografía)  - Glimmer-2 `#F2CEDB`  

4. [Arquitectura y Escena](#-arquitectura-y-escena)  - Glimmer-3 `#BF658F`  

5. [Requisitos](#-requisitos)  - Glimmer-4 `#BDF2D4` (botones)  

6. [Instalación y Configuración](#-instalación-y-configuración)  - Glimmer-5 `#D9C8A9`

7. [Configurar Tarjetas y Audio](#-configurar-tarjetas-y-audio)

8. [Uso de la Aplicación](#-uso-de-la-aplicación)## 🧰 Stack

9. [Build para Android](#-build-para-android)- **Unity** (versión usada en este repo).

10. [Solución de Problemas](#-solución-de-problemas)- **Vuforia Engine 9.8** (por el Virtual Button clásico).

11. [Licencias](#-licencias)- **TextMeshPro** (fuentes y IPA).

- **Git LFS** para audios/imágenes/modelos.

---

## 📁 Estructura relevante

## 🎯 Resumen del Proyecto

- **Nombre:** AR-Flashcards-Pronunciation
- **Tipo:** Aplicación educativa de Realidad Aumentada
- **Motor:** Unity 2020/2021 (recomendado)
- **AR SDK:** Vuforia Engine **9.8** (por compatibilidad con Virtual Button clásico)

### Descripción

Aplicación móvil de realidad aumentada diseñada para el aprendizaje interactivo de vocabulario en inglés. Los usuarios escanean tarjetas físicas (Image Targets) y, al presionar el Virtual Button integrado, acceden a un panel informativo que muestra el nombre en inglés, la transcripción fonética (IPA) y audio de pronunciación nativa.

---

## ✨ Características Principales

- **Detección de Image Targets** con Vuforia 9.8
- **Virtual Button** por tarjeta para mostrar/ocultar overlay de información
- **Audio por palabra:** clip específico asignado por tarjeta y/o fallback automático desde `Assets/Resources/tts/<palabra>.ogg`
- **HUD siempre visible:** indica estado "Apunta a una tarjeta / ¡Detectado!"
- **UI moderna:** botones redondeados, chip translúcido, animación de fade
- **Soporte de IPA:** transcripción fonética con TextMeshPro + fallback de fuente especializada
- **Paleta de color "Glimmer":** diseño visual coherente y atractivo
- **Modo retrato:** optimizado para uso móvil vertical

---

## 🎨 Paleta de Colores y Tipografía

### Paleta "Glimmer"

| Color       | Hex         | Uso                          |
|-------------|-------------|------------------------------|
| Glimmer-1   | `#A6586D`   | Acentos secundarios          |
| Glimmer-2   | `#F2CEDB`   | Fondos suaves                |
| Glimmer-3   | `#BF658F`   | Elementos destacados         |
| **Glimmer-4** | **`#BDF2D4`** | **Color primario**       |
| Glimmer-5   | `#D9C8A9`   | Detalles neutros             |

### Elementos UI Específicos

- **Chip translúcido:** Blanco con alpha ≈ 0.17 (`#FFFFFF2B`)
- **Overlay panel:** Negro con alpha ≈ 0.66 (`#000000AA`)

### Tipografía

- **Títulos/Botones:** Poppins, Montserrat o Inter (Bold/SemiBold)
- **IPA (Fonética):** Charis SIL o Doulos SIL (añadir como **Fallback** en Project Settings → TextMeshPro)
- **Recomendación:** Font Assets **Dynamic** en TMP para soporte completo de caracteres IPA

---

## 🏗️ Arquitectura y Escena

### Jerarquía Recomendada

```
Scene
├── ARCamera (con AudioListener)
├── HUDCanvas (Canvas Overlay)
│   ├── TopBar (opcional)
│   ├── ScanChip (chip de estado)
│   └── BottomDock (instrucciones)
├── Canvas (UI del overlay)
│   └── SafeArea
│       └── Panel (fondo oscuro con CanvasGroup)
│           └── Card (fondo blanco redondeado, sombra, layout vertical)
│               ├── Header (ChipWord + ReplayBtn)
│               ├── PhoneticText (TMP, IPA)
│               ├── Preview (Image opcional)
│               └── Footer (PronounceBtn + CloseBtn)
├── ImageTarget (scissors)
├── ImageTarget (clock)
├── ImageTarget (backpack)
├── ImageTarget (pencil)
├── ImageTarget (eraser)
└── ImageTarget (book)
```

### Scripts y Responsabilidades

#### **AROverlayController.cs**
Controla la visualización del overlay principal. Muestra/oculta el panel (con fade opcional), establece textos dinámicos y reproduce audio (clip asignado o fallback por nombre).

#### **AudioPronouncer.cs**
Reproduce AudioClip directo mediante `SayClip()` o busca automáticamente en `Resources/tts/<englishName>` mediante `Say()`.

#### **OverlayTrackableEventHandler.cs**
Hereda de `DefaultTrackableEventHandler`. 
- **OnTrackingFound:** Establece `englishName`, `phonetic` y opcional `voiceClip` en `AROverlayController`; notifica al HUD con `SetFound`.
- **OnTrackingLost:** Ejecuta `HideOverlay` y notifica al HUD con `SetLost`. 
- ⚠️ **Importante:** No abre el panel automáticamente (solo el Virtual Button lo hace).

#### **ButtonManager.cs**
Maneja el Virtual Button de cada tarjeta:
- **OnPressed:** Ejecuta `ShowOverlay()`
- **OnReleased:** Ejecuta `HideOverlay()`

💡 **Consejo:** Si el dedo tapa el target y hay un `lost` breve, usar un flag para no cerrar el overlay si el VB sigue presionado.

#### **ScanHUDController.cs**
Cambia el estado del HUD visual:
- `SetScanning()`: "Apunta a una tarjeta"
- `SetFound(nombre)`: "¡[Nombre] detectado!"
- `SetLost()`: Vuelve al estado de escaneo

#### **PulseUI.cs** _(opcional)_
Añade animación de "respiración" sutil al ScanChip para mejorar feedback visual.

#### **ThemeApplier.cs** _(opcional)_
Aplica colores de la paleta Glimmer a botones, cards, textos y otros elementos UI de forma centralizada.

---

## 📋 Requisitos

- **Unity:** 2020.3 LTS o 2021.3 LTS (indicar versión exacta usada en tu proyecto)
- **Vuforia Engine:** 9.8 (License Key configurada)
- **Android SDK/NDK:** Si compila para Android
- **TextMeshPro:** Importado y configurado (incluido por defecto en Unity)

---

## ⚙️ Instalación y Configuración

### 1. Configurar Vuforia

1. Agregar/activar **Vuforia Engine 9.8** en Unity
2. Colocar tu **License Key** en `Project Settings → Vuforia`
3. ⚠️ **Nota:** La versión gratuita muestra watermark de Vuforia

### 2. Configurar UI/Canvas

- **Canvas Scaler:**
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution: **1080×1920**
  - Match: **1.0** (modo retrato)
- ⚠️ **Importante:** El Panel del overlay debe estar **fuera** de los `ImageTarget` (en Canvas independiente)

### 3. Configurar Fuentes para IPA

1. Importar fuentes **Charis SIL** o **Doulos SIL** (disponibles en [SIL.org](https://software.sil.org/))
2. Crear Font Asset en TextMeshPro (menú contextual: Create → TextMeshPro → Font Asset)
3. Configurar como **Dynamic** para soporte completo de caracteres
4. Ir a `Project Settings → TextMeshPro → Settings`
5. En **Fallback Font Assets**, añadir el font asset de Charis/Doulos

### 4. Configurar Sprite Redondeado

- Importar `rounded_32.png` (o similar)
- Configurar como **9-Slice Sprite**
- Border: `32, 32, 32, 32`
- Usar para Card, Buttons y Chip

---

## 🎴 Configurar Tarjetas y Audio

### Configurar cada ImageTarget

1. Añadir componente `OverlayTrackableEventHandler`
2. Asignar campos:
   - **englishName:** Nombre en inglés (ej.: "Scissors")
   - **phonetic:** Transcripción IPA (ej.: "/ˈsɪz.ɚz/")
   - **voiceClip:** AudioClip específico (opcional)
   - **overlay:** Referencia a `AROverlayController` de la escena
   - **hud:** Referencia a `ScanHUDController` del HUDCanvas

### Configurar Virtual Button

En el componente `ButtonManager` de cada tarjeta:
- **OnButtonPressed:** Asignar método `AROverlayController.ShowOverlay()`
- **OnButtonReleased:** Asignar método `AROverlayController.HideOverlay()`

💡 **Tip anti-parpadeo:** Si hay pérdida de tracking al tapar con el dedo, usar un flag "VB presionado" y no cerrar en `OnTrackingLost` mientras el botón esté activo.

### Configurar Fallback de Audio

Colocar archivos de audio en `Assets/Resources/tts/` con nombres en **minúsculas**:

```
Assets/Resources/tts/
├── scissors.ogg
├── clock.ogg
├── backpack.ogg
├── pencil.ogg
├── eraser.ogg
└── book.ogg
```

Si no se asigna `voiceClip` específico, el sistema buscará automáticamente el archivo correspondiente.

---

## 🚀 Uso de la Aplicación

### Flujo de Usuario

1. **Apuntar a una tarjeta** → HUD muestra: "¡[Nombre] detectado!"
2. **Presionar el Virtual Button** → Se abre el overlay con:
   - Nombre en inglés
   - Transcripción fonética (IPA)
   - Audio de pronunciación automático
3. **Botón "Volver a pronunciar"** → Repite el audio
4. **Soltar el Virtual Button** → El overlay se cierra automáticamente

---

## 📱 Build para Android

### Pasos de Compilación

1. Ir a `File → Build Settings → Android`
2. Click en **Switch Platform**
3. Configurar **Player Settings:**
   - **Orientation:** Portrait
   - **Vuforia Engine:** Enabled
   - **Internet Access:** Not Required (audios locales)
   - **Minimum API Level:** Android 7.0 (API 24) o superior
4. Click en **Build** o **Build & Run**

### Requisitos del Dispositivo

- Android 7.0 (Nougat) o superior
- Cámara trasera funcional
- Mínimo 2GB RAM (recomendado)

---

## 🔧 Solución de Problemas

### El panel se abre automáticamente al detectar la tarjeta

**Causa:** Panel es hijo de un ImageTarget o se llama `ShowOverlay()` en `OnTrackingFound`.

**Solución:**
- Asegurar que el Panel está en un Canvas independiente (no hijo de ImageTarget)
- Verificar que `OnTrackingFound` **NO** llama a `ShowOverlay()` (solo el Virtual Button debe hacerlo)

### El panel no aparece al presionar el Virtual Button

**Causas posibles:**
- Campo `panel` no asignado en `AROverlayController`
- Panel desactivado en jerarquía

**Solución:**
- Verificar asignación del campo `panel` en Inspector
- Añadir logs en `ShowOverlay()` para debug
- Crear script `PanelGuard` con `OnEnable/OnDisable` para verificar estado

### No se reproduce el audio

**Causas posibles:**
- No hay `AudioListener` en ARCamera
- Fallback no encuentra archivo en `Resources/tts`
- `voiceClip` no asignado y nombre de archivo no coincide

**Solución:**
- Confirmar que ARCamera tiene componente `AudioListener`
- Revisar que archivos en `Resources/tts/` están en minúsculas
- Si usas `voiceClip`, verificar asignación en Inspector
- Verificar formato de audio compatible (OGG, MP3, WAV)

### Caracteres IPA se muestran como □□□

**Causa:** Falta configurar fallback fonts de TextMeshPro.

**Solución:**
- Importar Charis SIL o Doulos SIL
- Crear Font Asset como **Dynamic**
- Añadir en `Project Settings → TextMeshPro → Fallback Font Assets`

### Valores NaN en elementos UI

**Causas posibles:**
- Canvas Scaler mal configurado
- Content Size Fitter + Layout Group en el mismo GameObject
- RectTransform corrupto

**Solución:**
- Verificar Canvas Scaler: 1080×1920, Match = 1.0
- Evitar Content Size Fitter y Layout Group simultáneos
- Reset de RectTransform (clic derecho → Reset)

### Pérdida de tracking al presionar Virtual Button

**Causa:** El dedo tapa la tarjeta momentáneamente.

**Solución:**
- Implementar flag "buttonPressed" en `ButtonManager`
- En `OnTrackingLost`, verificar flag antes de cerrar overlay
- Solo cerrar si el botón no está presionado

---

## 📄 Licencias

### Vuforia Engine
- Sujeto a licencia de Vuforia (PTC Inc.)
- Versión gratuita incluye watermark visible
- Consultar [términos de Vuforia](https://developer.vuforia.com/legal/vuforia-developer-agreement)

### Fuentes
- **Google Fonts** (Poppins, Montserrat, Inter): [SIL Open Font License](https://scripts.sil.org/OFL)
- **Charis SIL / Doulos SIL**: [SIL Open Font License](https://scripts.sil.org/OFL)

### Audios
- Materiales con permisos adecuados (propios o con licencia libre)
- Si usas recursos de terceros, incluir atribución correspondiente

### Código
- Proyecto educativo - verificar términos con tu institución
- Si es código abierto, considerar licencia MIT o GPL v3

---

## 👥 Contribuciones

Este proyecto fue desarrollado como parte de un proyecto educativo. Si deseas contribuir o reportar problemas, por favor abre un issue en este repositorio.

---

## 📧 Contacto

Para preguntas o sugerencias sobre el proyecto, contactar a través de los canales institucionales correspondientes.

---

**Desarrollado con ❤️ usando Unity y Vuforia Engine**
