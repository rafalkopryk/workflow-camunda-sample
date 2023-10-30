export function createBpmnJSInstance(element) {
    return new BpmnJS({
        container: element,
        height: 500
    })
}

export async function openDiagram(viewer, bpmnXML) {

    // import diagram
    try {
        // import a BPMN 2.0 diagram
        try {
            // we did well!
            await viewer.importXML(bpmnXML);
            viewer.get('canvas').zoom('fit-viewport');

        } catch (err) {
            // import failed :-(
        }
    } catch (err) {
        console.error('could not import BPMN 2.0 diagram', err);
    }
}

export async function addMarker(viewer, progressIdsArray) {

    try {

        let canvas = viewer.get('canvas');
        progressIdsArray.forEach(key => canvas.addMarker(key, 'highlight'))

    } catch (err) {
        
    }
}