describe('Home page', () => {
    beforeEach(() => {
        cy.visit('/');
    }),
        it('loads as the default route', () => {
            cy.location().should(loc => {
                expect(loc.pathname).to.equal('/home');
            });
        }),
        it('has sidenav options', () => {
            cy.contains('Home');
            cy.contains('Leaders');
            cy.contains('Teams');
            cy.contains('Games');
            cy.contains('About');
            cy.contains('Admin');
        }),
        it('has a collapsible "All Games" card', () => {
            cy.contains('All Games').as('allGames').should('be.visible');
            cy.get('@allGames').parentsUntil('div').last().as('card').within(() => {
                cy.contains('Overall').should('be.visible');
                cy.contains('Batting').should('be.visible');
                cy.contains('Pitching').should('be.visible');
            });
            cy.get('@allGames').click();
            cy.get('@card').within(() => {
                cy.contains('Overall').should('not.be.visible');
                cy.contains('Batting').should('not.be.visible');
                cy.contains('Pitching').should('not.be.visible');
            });
        })
})