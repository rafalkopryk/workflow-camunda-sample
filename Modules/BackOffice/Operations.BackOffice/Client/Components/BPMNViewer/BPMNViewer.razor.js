
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

export function addElemenInstanceSequenceFlowsMarker(viewer, sequenceFlows) { 
    let canvas = viewer.get('canvas');
    sequenceFlows.forEach(key => canvas.addMarker(key, 'bpmn-element-active'))
}

export function addElementInstanceIncidentMarker(viewer, elementId) {
    let canvas = viewer.get('canvas');
    canvas.addMarker(elementId, 'bpmn-element-incident');
}

export function addIncidentMarker(viewer, elemenId, html) {
    let canvas = viewer.get('canvas');
    let overlays = viewer.get('overlays');

    overlays.add(elemenId, {
        position: {
            top: -10,
            right: 10
        },
        html: html
    });
}