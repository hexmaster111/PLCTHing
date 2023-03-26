namespace InstructionList;

public class InstructionListReader
{
    //Basic Start Stop Latching Circuit
    /*
     *  START_BUTTON OR MOTOR_CONTACT AND STOP_BUTTON = MOTOR_COIL
     *
     *
     *  DEC VAR_NUMBER_A : Number : 0
     *  DEC VAR_NUMBER_B : Number : 0
     * 
     *  START_BUTTON = [ADD VAR_NUMBER_A 1 VAR_NUMBER_A] //IF(START_BUTTON == TRUE) THEN NUMBER_A = NUMBER_A + 1
     *
     *   START_BUTTON AND (NUMBER_2 < NUMBER_1) = MOTOR
     *
     *       NAME :  TYPE  :  PRE  : ACC
     *  DEC TIMER0 : Timer :  1000 : 0
     *
     *  START_BUTTON = [TIMER TIMER0 ENABLE]
     *
     *  TIMER0.DN = MOTOR_COIL // IF(TIMER0.DN == TRUE) THEN MOTOR_COIL = TRUE
     *
     *      NAME    : TYPE : INIT
     *  DEC IS_DONE : Bool : False
     * 
     *  IF((START_BUTTON == TRUE OR MOTOR == TRUE) AND STOP_BUTTON != TRUE) THEN MOTOR_COIL = TRUE
     *  (START_BUTTON OR (MOTOR AND (C OR D))) AND !STOP_BUTTON = MOTOR_COIL //COMMENT HERE
     *
     *  
     *  (START OR (C AND D) OR (C AND C) AND !STOP) = MOTOR = XXX
     *  C OR C = XXX
     *
     *          NAME :  TYPE   : PRE: ACC
     *  DEC COUNTER0 : Counter : 20 : 0
     *
     *  START_BUTTON = [ADD COUNTER0 1 COUNTER0]
     *
     *
     * ------------- A MORE REALISTIC EXAMPLE -------------
     *
     *  DEC CurrentFloor : Number : 0
     *
     *  DEC GLOBAL_ENABLE : Bool : False : RAM   || COMMENT: This makes a bool named GLOBAL_ENABLE that is initialized to False and is stored in RAM
     *  DEC START_BUTTON : Bool : False : IPI[0] || COMMENT: This makes a bool named START_BUTTON that is initialized to False and is mapped to input Plugin interface slot 0
     *  DEC STOP_BUTTON : Bool : False : IPI[1]  || COMMENT: This makes a bool named STOP_BUTTON that is initialized to False and is mapped to input Plugin interface slot 1
     *  DEC MOTOR : Bool : False : OPI[2]        || COMMENT: This makes a bool named MOTOR that is initialized to False and is mapped to output Plugin interface slot 2
     *  DEC SOME_STATE : Bool : False : RAM     || COMMENT: This makes a bool named SOME_STATE that is initialized to False and is stored in RAM
     *  DEC SERVICE_REQUIRED : Bool : False : RAM || COMMENT: This makes a bool named SERVICE_REQUIRED that is initialized to False and is stored in RAM
     *
     *  DEC BITFIELD0 : Bitfield_8 : 0b00000000 : RAM      || COMMENT: This makes a bitfield named BITFIELD0 that is initialized to 0 and is stored in RAM
     * 
     *  DEC TIMES_STARTED : Counter : 0 : RAM   || COMMENT: This makes a counter named TIMES_STARTED that is initialized to 0 and is stored in RAM
     * 
     *  || NO Examine if open  (Think of this like Normally Open)
     *  || NC Examine if closed (Think of this like Normally Closed)
     *
     *
     *  :NO GLOBAL_ENABLE AND
     *     NO START_BUTTON AND
     *       NC MOTOR THEN
     *          TIMES_STARTED = +1;
     *
     * 
     *      || The ":" is used to show a new line,
     *      || "AND" is used to show that the next line is a continuation of the previous line,
     *      || This would logically be the same as IF(GLOBAL_ENABLE == TRUE) AND IF(START_BUTTON == TRUE OR MOTOR == TRUE) AND IF(STOP_BUTTON != TRUE) THEN MOTOR = TRUE;
     *      || Every comment abouve this statement is appart of the top level comment
     *      :NO GLOBAL_ENABLE AND                         || IF(GLOBAL_ENABLE == TRUE) AND
     *          NO START_BUTTON OR NO MOTOR AND           || IF(START_BUTTON == TRUE OR MOTOR == TRUE) AND
     *             NC STOP_BUTTON THEN                    || IF(STOP_BUTTON != TRUE) THEN
     *                  MOTOR = TRUE AND
     *                  SOME_STATE = TRUE;                || MOTOR = TRUE AND SOME_STATE = TRUE;
     *
     *     || The ";" Ends the statement
     *     || Note, that if this where to not evaluate to true, MOTOR would be set to false, and SOME_STATE would be set to false
     *
     *
     *      :NO TIMES_STARTED.GTR #10 THEN               || IF(TIMES_STARTED > 10) THEN
     *          SERVICE_REQUIRED = TRUE;                 || SERVICE_REQUIRED = TRUE;
     *      || This logic could also be written with a variable as follows:
     *
     *      DEC MAX_STARTS : Number : 10 : CONST          || This makes a number named MAX_STARTS that is initialized to 10 and is stored in CONST
     *      :NO TIMES_STARTED.GTR MAX_STARTS THEN         || IF(TIMES_STARTED > MAX_STARTS) THEN
     *         SERVICE_REQUIRED = TRUE;                 || SERVICE_REQUIRED = TRUE;
     *
     * 
     */

    public const string TestProgram = @"
DEC START_BUTTON : Bool : False : IPI[0]; || What index of the Input Plugin Interface this var is mapped to
DEC STOP_BUTTON : Bool : False : IPI[1];
DEC MOTOR : Bool : False : OPI[2];
DEC SOME_STATE : Bool : False : RAM;

|| NO Examine if open  (Think of this like Normally Open)
:NO START_BUTTON OR MOTOR AND   || Motor or Start button
      NO STOP_BUTTON THEN || This is a Logic Statement
         MOTOR = TRUE AND       || Motor and ( NOTE A COMMENT CANT BE ON THE LAST LINE)
         SOME_STATE = TRUE;     
|| More notes about the below statement
: NO START_BUTTON AND SOME_STATE THEN
    MOTOR = FALSE AND
    SOME_STATE = FALSE;
";


    public bool Read(out InstructionListProgram program, out string[] errors)
    {
        program = new InstructionListProgram();
        var programText = TestProgram;

        var isOk = Declaration.ParseFile(programText, out program.Declarations, out errors, out var withoutDeclarations);
        if (!isOk) return false;

        return true;
    }
}

public enum TokenTypes
{
    DeclarationStart, // "DEC" Declaration of a variable
    DeclarationName, // Name of the variable
    DeclarationType, // Type of the variable
    DeclarationSeparator, // ":" Separation of the variable name, type, initial value, and storage
    DeclarationInit, // Initial value of the variable
    DeclarationStorage, // Where the variable is stored
    DeclarationEnd, // ";" End of a declaration
    Comment, // Comments are ignored, they start with "||" and end with a new line
    StatementStart, // ":" Start of a logical statement
    StatementVariable, // Name of the variable in a logical statement line
    StatementOr, // "OR" Separation of logical statements
    StatementAnd, // "AND" Separation of logical statements
    StatementThen, // "THEN" Separation of logical statements 
    StatementEnd, // ";" End of a logical statement
}

public class InstructionListProgram
{
    public Declaration[] Declarations;
}

