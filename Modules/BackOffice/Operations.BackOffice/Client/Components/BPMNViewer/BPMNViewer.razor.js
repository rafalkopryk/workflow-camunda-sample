
export function createBpmnJSInstance(element) {
    return new BpmnJS({
        container: element,
        height: 450
    })
}

export async function openDiagram(viewer, bpmnXML) {

    // import diagram
    try {
        // import a BPMN 2.0 diagram
        try {
            // we did well!
            await viewer.importXML(bpmnXML);
            viewer.get('canvas').zoom('fit-viewport', 'auto');

        } catch (err) {
            // import failed :-(
        }
    } catch (err) {
        console.error('could not import BPMN 2.0 diagram', err);
    }
}


export function recenterDiagram(viewer) {

    //let canvas = viewer.get('canvas');
    //canvas.zoom("fit-viewport", "auto")
    //canvas.zoom("fit-viewport", "auto") 
}

export function addMarker(viewer, progressIdsArray) {

    try {

        let canvas = viewer.get('canvas');
        progressIdsArray.forEach(key => canvas.addMarker(key, 'highlight'))

    } catch (err) {
        
    }
}


export function stepZoom(viewer, value) {

    try {

        const zoomScroll = viewer.get('zoomScroll');

        zoomScroll.stepZoom(value);
    } catch (err) {

    }
}


export function addOverlay(overlayConfig, bpmnJsInstance) {
    let overlays = bpmnJsInstance.get('overlays')
    //@TODO add issue to bpmnjs to ask for position allowing all 4 props and can provide null to ignore.

    let position = {}
    if (overlayConfig.positionTop != null) {
        position.top = overlayConfig.positionTop
    }
    if (overlayConfig.positionLeft != null) {
        position.left = overlayConfig.positionLeft
    }
    if (overlayConfig.positionBottom != null) {
        position.bottom = overlayConfig.positionBottom
    }
    if (overlayConfig.positionRight != null) {
        position.right = overlayConfig.positionRight
    }
    overlayConfig.htmlElementRef.style.display = ''

    let id = overlays.add(overlayConfig.elementId,
        {
            position: position,
            html: overlayConfig.htmlElementRef
        }
    )

    return id
}

export function removeOverlays(overlayIds, bpmnJsInstance) {
    let overlays = bpmnJsInstance.get("overlays")
    overlayIds.forEach(id => {
        console.log("removing " + id)
        overlays.remove(id)
    })
}