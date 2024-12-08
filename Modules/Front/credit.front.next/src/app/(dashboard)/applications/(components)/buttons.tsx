"use client"

import { Button } from "@mui/material";
import Grid from '@mui/material/Grid2';
import { WithCreditApplicationState } from "../(dto)/creditApplicationDto";

export function SignContractButton({ creditApplication, onSignContract }: { creditApplication: WithCreditApplicationState, onSignContract: () => Promise<void> }) {

    const isContractSignedAllowed = creditApplication.state.level === 'DecisionGenerated' && creditApplication.state.decision === 'Positive'

    return isContractSignedAllowed &&
        (<Button
            variant="contained"
            color="primary"
            onClick={onSignContract}>
            Sign Contract
        </Button>);
}

export function CancelApplicationButton({ creditApplication, onCancelApplication }: { creditApplication: WithCreditApplicationState, onCancelApplication: () => Promise<void> }) {

    const isCancelApplicationAllowed = creditApplication.state.level === 'ApplicationRegistered' || creditApplication.state.level === 'DecisionGenerated'

    return isCancelApplicationAllowed &&
        (<Button
            variant="contained"
            color="primary"
            onClick={onCancelApplication}>
            Cancel Application
        </Button>);
}

export function RegisterApplicationButton({ onRegisterApplication }: { onRegisterApplication: () => Promise<void> }) {
    return <Grid size={{ xs: 12 }}>
        <Button variant="contained" color="primary" onClick={onRegisterApplication}>
            Register
        </Button>
    </Grid>
}