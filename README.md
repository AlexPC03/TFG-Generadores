# Generación Procedural de Mundos Virtuales - TFG

Este repositorio contiene el código fuente y los recursos asociados al Trabajo de Fin de Grado titulado **"Sistema modular de generación procedural de mundos virtuales"**, desarrollado por **Alejandro Pérez Carretero** para el **Grado en Diseño y Desarrollo de Videojuegos** en la **Escuela Técnica Superior de Ingeniería Informática**.

## Descripción

El proyecto implementa un sistema de generación procedural capaz de construir automáticamente distintas capas de un mundo virtual, desde la geografía física hasta la estructura urbana y política. Todo el sistema ha sido desarrollado en **Unity 2022.3.3f1** y tiene un enfoque conceptual.

## Funcionalidades principales

- Generación de terreno mediante múltiples capas de **ruido de Perlin**.
- Creación de **ríos** que fluyen siguiendo la pendiente del terreno.
- Distribución lógica de **recursos naturales** (campos, bosques, pesca, minas).
- Ubicación estratégica de **ciudades**, con especialización económica automática.
- Establecimiento de **relaciones diplomáticas** y formación de **países** mediante diagramas de **Voronoi**.
- Generación interna del trazado urbano de cada ciudad con un algoritmo basado en **Wave Function Collapse**.
- Configuración completa desde el editor de Unity, sin necesidad de modificar código.

## Estructura del proyecto

```
├──Assets/
├  ├── Delaunay/ # Conjunto de herramientas del repositorio "unity-delaunay"
├  ├── Materials/ # Materiales utilizados en el proyecto
├  ├── Models/ # Modelos 3D utilizados
├  ├── Prefabs/ # Prefabs para ciudades, caminos y edificios
├  ├── ProceduralTerrainPainter/ # Conjunto de herramientas del paquete de la Unity Asset Store "Procedural Terrain Painter"
├  ├── Scenes/ # Escena principal
├  ├── Scripts/ # Scripts principales del sistema
├  ├── Settings/ # Archivos de configuración de renderizado
├  ├── Sprites/ # Sprites 2D utilizados
├──Blend/ # Modelos .blend
├──Packages/ # Info de los paquetes de Unity
├──ProjectSettings # Archivos de Configuración del editor de Unity
```

## Requisitos

- Unity **2022.3.3f1** o compatible
- Paquetes necesarios:
  - "Procedural Terrain Painter" (Unity Asset Store)

##  Instrucciones de ejecución

1. Clona el repositorio o descárgalo como ZIP.
2. Abre el proyecto con la versión de Unity recomendada.
3. Abre la escena principal desde la carpeta `Scenes/`.
4. Se pueden Configurar los parámetros desde el inspector en los scripts correspondientes.
5. Pulsa "Play" para ejecutar la generación del mundo.

## Autor

- **Nombre:** Alejandro Pérez Carretero   
- **Universidad:** Universidad Rey Juan Carlos  
- **Grado:** Diseño y Desarrollo de Videojuegos  
- **Año:** 2025

## Herramientas externas

- Sigvardsson, O. (2019). "unity-delaunay". GitHub. https://github.com/OskarSigvardsson/unity-delaunay
- Procedural Terrain Painter. Unity Asset Store. https://assetstore.unity.com/packages/tools/terrain/procedural-terrain-painter-188357
